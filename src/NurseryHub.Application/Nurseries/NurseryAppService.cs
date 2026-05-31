using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Security;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
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

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var logoData = stream.ToArray();

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? ResolveContentTypeFromFileName(file.FileName)
            : file.ContentType;

        entity.SetLogo(logoData, contentType);
        await Repository.UpdateAsync(entity);

        return MapToGetOutputDto(entity);
    }

    [AllowAnonymous]
    public virtual async Task<IRemoteStreamContent> GetLogoAsync(Guid id)
    {
        Nursery entity;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            entity = await Repository.FindAsync(id)
                ?? throw new EntityNotFoundException(typeof(Nursery), id);
        }

        if (!entity.HasLogo)
        {
            throw new EntityNotFoundException(typeof(Nursery), id);
        }

        return new RemoteStreamContent(
            new MemoryStream(entity.LogoData!),
            contentType: entity.LogoContentType ?? "application/octet-stream",
            disposeStream: true);
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
                    HasLogo = nursery.LogoData != null && nursery.LogoData.Length > 0,
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
                LogoUrl = NurseryLogoUrlHelper.BuildLogoUrl(_mediaOptions.PublicBaseUrl, x.Id, x.HasLogo),
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
            LogoUrl = NurseryLogoUrlHelper.BuildLogoUrl(_mediaOptions.PublicBaseUrl, entity.Id, entity.HasLogo),
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

    private static string ResolveContentTypeFromFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".jfif" or ".jpe" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".tif" or ".tiff" => "image/tiff",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".avif" => "image/avif",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            _ => "application/octet-stream",
        };
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
