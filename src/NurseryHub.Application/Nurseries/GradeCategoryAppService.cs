using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Localization;
using NurseryHub.Permissions;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

[Authorize(Roles = NurseryHubRoles.Admin)]
public class GradeCategoryAppService
    : CrudAppService<
        GradeCategory,
        GradeCategoryDto,
        Guid,
        GetGradeCategoriesInput,
        CreateUpdateGradeCategoryDto,
        CreateUpdateGradeCategoryDto>,
        IGradeCategoryAppService
{
    private readonly IRepository<NurseryClass, Guid> _nurseryClassRepository;

    public GradeCategoryAppService(
        IRepository<GradeCategory, Guid> repository,
        IRepository<NurseryClass, Guid> nurseryClassRepository) : base(repository)
    {
        _nurseryClassRepository = nurseryClassRepository;
        LocalizationResource = typeof(NurseryHubResource);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var classCount = await _nurseryClassRepository.CountAsync(c => c.GradeCategoryId == id);
        if (classCount > 0)
        {
            var category = await Repository.GetAsync(id);
            throw new UserFriendlyException(L["NurseryHub:GradeCategories:CannotDeleteHasClasses", category.Name]);
        }

        await base.DeleteAsync(id);
    }

    protected override string? GetPolicyName { get; set; } = NurseryHubPermissions.GradeCategories.Default;
    protected override string? GetListPolicyName { get; set; } = NurseryHubPermissions.GradeCategories.Default;
    protected override string? CreatePolicyName { get; set; } = NurseryHubPermissions.GradeCategories.Create;
    protected override string? UpdatePolicyName { get; set; } = NurseryHubPermissions.GradeCategories.Edit;
    protected override string? DeletePolicyName { get; set; } = NurseryHubPermissions.GradeCategories.Delete;

    public override async Task<PagedResultDto<GradeCategoryDto>> GetListAsync(GetGradeCategoriesInput input)
    {
        var categories = await Repository.GetQueryableAsync();
        var classes = await _nurseryClassRepository.GetQueryableAsync();

        var filtered = categories
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var query = filtered
            .OrderBy(x => x.Name)
            .Select(x => new GradeCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Icon = x.Icon,
                ColorToken = x.ColorToken,
                Description = x.Description,
                IsActive = x.IsActive,
                ClassCount = classes.Count(c => c.GradeCategoryId == x.Id),
                CreationTime = x.CreationTime,
                CreatorId = x.CreatorId,
                LastModificationTime = x.LastModificationTime,
                LastModifierId = x.LastModifierId,
                IsDeleted = x.IsDeleted,
                DeleterId = x.DeleterId,
                DeletionTime = x.DeletionTime
            });

        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<GradeCategoryDto>(totalCount, items);
    }

    protected override Task<GradeCategory> MapToEntityAsync(CreateUpdateGradeCategoryDto createInput)
    {
        return Task.FromResult(
            new GradeCategory(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                createInput.Name,
                createInput.Icon,
                createInput.ColorToken,
                createInput.Description,
                createInput.IsActive));
    }

    protected override Task MapToEntityAsync(CreateUpdateGradeCategoryDto updateInput, GradeCategory entity)
    {
        entity.SetName(updateInput.Name);
        entity.SetIcon(updateInput.Icon);
        entity.SetColorToken(updateInput.ColorToken);
        entity.SetDescription(updateInput.Description);
        entity.SetIsActive(updateInput.IsActive);
        return Task.CompletedTask;
    }

    protected override GradeCategoryDto MapToGetOutputDto(GradeCategory entity)
    {
        return new GradeCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Icon = entity.Icon,
            ColorToken = entity.ColorToken,
            Description = entity.Description,
            IsActive = entity.IsActive,
            ClassCount = 0,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime
        };
    }
}
