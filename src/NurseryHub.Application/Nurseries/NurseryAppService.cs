using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

[Authorize(Roles = NurseryHubRoles.Admin)]
public class NurseryAppService
    : CrudAppService<
            Nursery,
            NurseryDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateNurseryDto,
            CreateUpdateNurseryDto>,
        INurseryAppService
{
    private readonly NurseryMediaOptions _mediaOptions;
    private readonly IRepository<NurseryBranch, Guid> _branchRepository;

    public NurseryAppService(
        IRepository<Nursery, Guid> repository,
        IRepository<NurseryBranch, Guid> branchRepository,
        IOptions<NurseryMediaOptions> mediaOptions)
        : base(repository)
    {
        _branchRepository = branchRepository;
        _mediaOptions = mediaOptions.Value;
    }


    public virtual async Task<NurseryDto> UploadLogoAsync(Guid id, UploadNurseryLogoInput input)
    {
        var entity = await Repository.GetAsync(id);
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

    public override async Task<PagedResultDto<NurseryDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var nurseries = await Repository.GetQueryableAsync();
        var branches = await _branchRepository.GetQueryableAsync();

        var query =
            from nursery in nurseries
            join branch in branches on nursery.Id equals branch.NurseryId into branchGroup
            select new
            {
                nursery.Id,
                nursery.Name,
                nursery.PhoneNumber,
                nursery.Email,
                nursery.LogoUrl,
                nursery.WebsiteUrl,
                nursery.IsActive,
                BranchCount = branchGroup.Count(),
                nursery.CreationTime,
                nursery.CreatorId,
                nursery.LastModificationTime,
                nursery.LastModifierId,
                nursery.IsDeleted,
                nursery.DeleterId,
                nursery.DeletionTime,
            };

        var totalCount = await AsyncExecuter.CountAsync(query);
        var rawItems = await AsyncExecuter.ToListAsync(
            query.OrderBy(n => n.Name).Skip(input.SkipCount).Take(input.MaxResultCount));
        var items = rawItems.Select(x => new NurseryDto
        {
            Id = x.Id,
            Name = x.Name,
            PhoneNumber = x.PhoneNumber,
            Email = x.Email,
            LogoUrl = ResolveLogoDisplayUrl(x.LogoUrl),
            WebsiteUrl = x.WebsiteUrl,
            IsActive = x.IsActive,
            BranchCount = x.BranchCount,
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

    public override async Task<NurseryDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var branches = await _branchRepository.GetQueryableAsync();

        var dto = MapToGetOutputDto(entity);
        dto.BranchCount = await AsyncExecuter.CountAsync(branches.Where(x => x.NurseryId == id));
        return dto;
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
                createInput.IsActive));
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
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            LogoUrl = ResolveLogoDisplayUrl(entity.LogoUrl),
            WebsiteUrl = entity.WebsiteUrl,
            IsActive = entity.IsActive,
            BranchCount = 0,
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
}
