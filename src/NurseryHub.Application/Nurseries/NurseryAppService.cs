using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace NurseryHub.Nurseries;

[Authorize(Roles = NurseryHubRoles.Admin)]
public class NurseryAppService
    : CrudAppService<
            Nursery,
            NurseryDto,
            Guid,
            GetNurseriesInput,
            CreateUpdateNurseryDto,
            CreateUpdateNurseryDto>,
        INurseryAppService
{
    private readonly NurseryMediaOptions _mediaOptions;
    private readonly IRepository<NurseryBranch, Guid> _branchRepository;
    private readonly IRepository<IdentityUser, Guid> _identityUserRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly TenantManager _tenantManager;
    private readonly IDataFilter _dataFilter;

    public NurseryAppService(
        IRepository<Nursery, Guid> repository,
        IRepository<NurseryBranch, Guid> branchRepository,
        IRepository<IdentityUser, Guid> identityUserRepository,
        ITenantRepository tenantRepository,
        TenantManager tenantManager,
        IDataFilter dataFilter,
        IOptions<NurseryMediaOptions> mediaOptions)
        : base(repository)
    {
        _branchRepository = branchRepository;
        _identityUserRepository = identityUserRepository;
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _dataFilter = dataFilter;
        _mediaOptions = mediaOptions.Value;
    }

    public override async Task<NurseryDto> CreateAsync(CreateUpdateNurseryDto input)
    {
        var tenant = await CreateTenantForNurseryAsync(input.NurseryCode);
        var nursery = new Nursery(
                GuidGenerator.Create(),
                tenant.Id,
                input.Name,
                input.PhoneNumber,
                input.Email,
                logoUrl: null,
                input.WebsiteUrl,
                input.IsActive)
        {
            CreatorId = CurrentUser.Id
        };

        nursery = await Repository.InsertAsync(nursery, autoSave: true);

        var dto = MapToGetOutputDto(nursery);
        dto.NurseryCode = tenant.Name;
        return dto;
    }


    public virtual async Task<NurseryDto> UploadLogoAsync(Guid id, UploadNurseryLogoInput input)
    {
        var entity = await GetEntityByIdAsync(id);
        var file = input.File;

        var ext = Path.GetExtension(file.FileName)!.ToLowerInvariant();

        var directory = _mediaOptions.LogoPhysicalPath;
        Directory.CreateDirectory(directory);

        DeleteStoredLogoFileIfExists(entity.LogoUrl);

        var fileName = $"{GuidGenerator.Create()}{ext}";
        var physicalPath = Path.Combine(directory, fileName);

        await using (var stream = File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        entity.SetLogoUrl(fileName);
        await Repository.UpdateAsync(entity);

        return MapToGetOutputDto(entity);
    }

    public override async Task<PagedResultDto<NurseryDto>> GetListAsync(GetNurseriesInput input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var nurseries = await Repository.GetQueryableAsync();
            var branches = await _branchRepository.GetQueryableAsync();
            var identityUsers = await _identityUserRepository.GetQueryableAsync();

            var filteredNurseries = nurseries;
            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                filteredNurseries = filteredNurseries.Where(n => n.Name.Contains(input.Name));
            }

            if (!string.IsNullOrWhiteSpace(input.Email))
            {
                filteredNurseries = filteredNurseries.Where(n => n.Email.Contains(input.Email));
            }

            if (input.IsActive.HasValue)
            {
                filteredNurseries = filteredNurseries.Where(n => n.IsActive == input.IsActive.Value);
            }

            if (input.CreationDateFrom.HasValue)
            {
                var from = input.CreationDateFrom.Value.Date;
                filteredNurseries = filteredNurseries.Where(n => n.CreationTime >= from);
            }

            if (input.CreationDateTo.HasValue)
            {
                var toExclusive = input.CreationDateTo.Value.Date.AddDays(1);
                filteredNurseries = filteredNurseries.Where(n => n.CreationTime < toExclusive);
            }

            // Correlated Count subqueries — EF translates these reliably; double GroupJoin + Count does not.
            var query = filteredNurseries
                .OrderBy(nursery => nursery.Name)
                .Select(nursery => new
                {
                    nursery.Id,
                    nursery.TenantId,
                    nursery.Name,
                    nursery.PhoneNumber,
                    nursery.Email,
                    nursery.LogoUrl,
                    nursery.WebsiteUrl,
                    nursery.IsActive,
                    BranchCount = branches.Count(b => b.NurseryId == nursery.Id),
                    UserCount = identityUsers.Count(u => u.TenantId == nursery.TenantId),
                    nursery.CreationTime,
                    nursery.CreatorId,
                    nursery.LastModificationTime,
                    nursery.LastModifierId,
                    nursery.IsDeleted,
                    nursery.DeleterId,
                    nursery.DeletionTime,
                });

            var totalCount = await AsyncExecuter.CountAsync(filteredNurseries);
            var rawItems = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var tenantIds = rawItems
                .Where(x => x.TenantId.HasValue)
                .Select(x => x.TenantId!.Value)
                .Distinct()
                .ToList();
            var tenantCodeMap = new Dictionary<Guid, string>();
            foreach (var tenantId in tenantIds)
            {
                var tenant = await _tenantRepository.FindAsync(tenantId);
                if (tenant != null)
                {
                    tenantCodeMap[tenantId] = tenant.Name;
                }
            }
            var items = rawItems.Select(x => new NurseryDto
            {
                Id = x.Id,
                Name = x.Name,
                NurseryCode = x.TenantId.HasValue && tenantCodeMap.TryGetValue(x.TenantId.Value, out var tenantCode)
                    ? tenantCode
                    : null,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                LogoUrl = ResolveLogoDisplayUrl(x.LogoUrl),
                WebsiteUrl = x.WebsiteUrl,
                IsActive = x.IsActive,
                BranchCount = x.BranchCount,
                UserCount = x.UserCount,
                CreationTime = x.CreationTime,
                CreatorId = x.CreatorId,
                LastModificationTime = x.LastModificationTime,
                LastModifierId = x.LastModifierId,
                IsDeleted = x.IsDeleted,
                DeleterId = x.DeleterId,
                DeletionTime = x.DeletionTime,
            }).ToList();

            return new PagedResultDto<NurseryDto>(totalCount, items);
        }
    }

    public override async Task<NurseryDto> GetAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var branches = await _branchRepository.GetQueryableAsync();

            var dto = MapToGetOutputDto(entity);
            dto.BranchCount = await AsyncExecuter.CountAsync(branches.Where(x => x.NurseryId == id));
            var identityUsers = await _identityUserRepository.GetQueryableAsync();
            if (entity.TenantId.HasValue)
            {
                dto.UserCount = await AsyncExecuter.CountAsync(
                    identityUsers.Where(u => u.TenantId == entity.TenantId));
                var tenant = await _tenantRepository.FindAsync(entity.TenantId.Value);
                dto.NurseryCode = tenant?.Name;
            }

            return dto;
        }
    }

    public override async Task DeleteAsync(Guid id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            await Repository.DeleteAsync(id);
        }
    }

    protected override async Task<Nursery> GetEntityByIdAsync(Guid id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await Repository.GetAsync(id);
        }
    }

    protected override async Task<Nursery> MapToEntityAsync(CreateUpdateNurseryDto createInput)
    {
        return await Task.FromResult(
            new Nursery(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                createInput.Name,
                createInput.PhoneNumber,
                createInput.Email,
                logoUrl: null,
                createInput.WebsiteUrl,
                createInput.IsActive)
            {
                CreatorId = CurrentUser.Id
            });
    }

    protected override async Task MapToEntityAsync(CreateUpdateNurseryDto updateInput, Nursery entity)
    {
        entity.SetName(updateInput.Name);
        entity.SetPhoneNumber(updateInput.PhoneNumber);
        entity.SetEmail(updateInput.Email);
        entity.SetWebsiteUrl(updateInput.WebsiteUrl);
        entity.SetIsActive(updateInput.IsActive);
        await Task.CompletedTask;
    }

    protected override NurseryDto MapToGetOutputDto(Nursery entity)
    {
        return new NurseryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            NurseryCode = null,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            LogoUrl = ResolveLogoDisplayUrl(entity.LogoUrl),
            WebsiteUrl = entity.WebsiteUrl,
            IsActive = entity.IsActive,
            BranchCount = 0,
            UserCount = 0,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime,
        };
    }

    protected override IQueryable<Nursery> ApplyDefaultSorting(IQueryable<Nursery> query)
    {
        return query.OrderBy(n => n.Name);
    }

    private string? ResolveLogoDisplayUrl(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return null;
        }

        if (stored.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            stored.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return stored;
        }

        var baseUrl = _mediaOptions.PublicBaseUrl.TrimEnd('/');
        var fileName = stored.Replace('\\', '/').TrimStart('/');
        if (fileName.Contains('/', StringComparison.Ordinal))
        {
            return $"{baseUrl}/{fileName}";
        }

        return $"{baseUrl}/logo/{fileName}";
    }

    private void DeleteStoredLogoFileIfExists(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return;
        }

        if (stored.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            stored.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var name = Path.GetFileName(stored.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        var path = Path.Combine(_mediaOptions.LogoPhysicalPath, name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private async Task<Tenant> CreateTenantForNurseryAsync(string? requestedNurseryCode)
    {
        var requestedOrGeneratedCode = string.IsNullOrWhiteSpace(requestedNurseryCode)
            ? await GenerateReadableNurseryCodeAsync()
            : requestedNurseryCode;
        var uniqueTenantName = await BuildUniqueTenantNameAsync(requestedOrGeneratedCode);
        var tenant = await _tenantManager.CreateAsync(uniqueTenantName);
        return await _tenantRepository.InsertAsync(tenant, autoSave: true);
    }

    private async Task<string> BuildUniqueTenantNameAsync(string? requestedNurseryCode)
    {
        const int maxLength = 32;
        var baseCode = NormalizeTenantCode(requestedNurseryCode);

        if (string.IsNullOrWhiteSpace(baseCode))
        {
            baseCode = "n1001";
        }

        var candidate = baseCode;
        var suffix = 1;

        while (await _tenantRepository.FindByNameAsync(candidate) != null)
        {
            var suffixText = $"-{suffix}";
            var trimmedBase = baseCode.Length + suffixText.Length > maxLength
                ? baseCode[..(maxLength - suffixText.Length)]
                : baseCode;
            candidate = $"{trimmedBase}{suffixText}";
            suffix++;
        }

        return candidate;
    }

    private static string? NormalizeTenantCode(string? requestedNurseryCode)
    {
        if (string.IsNullOrWhiteSpace(requestedNurseryCode))
        {
            return null;
        }

        var normalized = new string(requestedNurseryCode
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        normalized = normalized.Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (!char.IsLetter(normalized[0]))
        {
            normalized = $"n-{normalized}";
        }

        return normalized.Length > 32 ? normalized[..32].Trim('-') : normalized;
    }

    private async Task<string> GenerateReadableNurseryCodeAsync()
    {
        const string prefix = "n";
        var seed = 1000;
        string candidate;

        do
        {
            seed++;
            candidate = $"{prefix}{seed}";
        } while (await _tenantRepository.FindByNameAsync(candidate) != null);

        return candidate;
    }

}
