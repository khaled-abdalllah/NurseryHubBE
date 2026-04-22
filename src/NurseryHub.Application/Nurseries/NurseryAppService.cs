using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;


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

    public NurseryAppService(
        IRepository<Nursery, Guid> repository,
        IOptions<NurseryMediaOptions> mediaOptions)
        : base(repository)
    {
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
