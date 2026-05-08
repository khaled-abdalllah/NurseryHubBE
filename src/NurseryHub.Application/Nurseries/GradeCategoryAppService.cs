using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Localization;
using NurseryHub.Permissions;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
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
    private readonly IRepository<NurseryBranch, Guid> _nurseryBranchRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;

    public GradeCategoryAppService(
        IRepository<GradeCategory, Guid> repository,
        IRepository<NurseryClass, Guid> nurseryClassRepository,
        IRepository<NurseryBranch, Guid> nurseryBranchRepository,
        IRepository<UserBranch, Guid> userBranchRepository) : base(repository)
    {
        _nurseryClassRepository = nurseryClassRepository;
        _nurseryBranchRepository = nurseryBranchRepository;
        _userBranchRepository = userBranchRepository;
        LocalizationResource = typeof(NurseryHubResource);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var category = await Repository.GetAsync(id);
        await EnsureCanAccessBranchAsync(category.NurseryBranchId);

        var classCount = await _nurseryClassRepository.CountAsync(c => c.GradeCategoryId == id);
        if (classCount > 0)
        {
            throw new UserFriendlyException(L["NurseryHub:GradeCategories:CannotDeleteHasClasses", category.Name]);
        }

        await base.DeleteAsync(id);
    }

    public override async Task<GradeCategoryDto> GetAsync(Guid id)
    {
        var category = await Repository.GetAsync(id);
        await EnsureCanAccessBranchAsync(category.NurseryBranchId);
        return await base.GetAsync(id);
    }



    public override async Task<PagedResultDto<GradeCategoryDto>> GetListAsync(GetGradeCategoriesInput input)
    {
        var accessibleBranchIds = await GetAccessibleBranchIdsAsync(input.NurseryBranchId);
        var categories = await Repository.GetQueryableAsync();
        var classes = await _nurseryClassRepository.GetQueryableAsync();

        var filtered = categories
            .Where(x => accessibleBranchIds.Contains(x.NurseryBranchId))
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var classScope = classes.Where(c => accessibleBranchIds.Contains(c.NurseryBranchId));

        var query = filtered
            .OrderBy(x => x.Name)
            .Select(x => new GradeCategoryDto
            {
                Id = x.Id,
                NurseryBranchId = x.NurseryBranchId,
                Name = x.Name,
                Icon = x.Icon,
                ColorToken = x.ColorToken,
                Description = x.Description,
                IsActive = x.IsActive,
                ClassCount = classScope.Count(c => c.GradeCategoryId == x.Id),
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

    private async Task<Guid[]> GetAccessibleBranchIdsAsync(Guid? requestedBranchId)
    {
        if (requestedBranchId.HasValue)
        {
            await EnsureCanAccessBranchAsync(requestedBranchId.Value);
            return new[] { requestedBranchId.Value };
        }

        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            return Array.Empty<Guid>();
        }

        var userBranches = await _userBranchRepository.GetListAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value);
        return userBranches.Select(x => x.NurseryBranchId).Distinct().ToArray();
    }

    private async Task EnsureCanAccessBranchAsync(Guid nurseryBranchId)
    {
        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }

        var canAccess = await _userBranchRepository.AnyAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value &&
            x.NurseryBranchId == nurseryBranchId);
        if (!canAccess)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }
    }

    protected override Task<GradeCategory> MapToEntityAsync(CreateUpdateGradeCategoryDto createInput)
    {
        return MapCreateEntityAsync(createInput);
    }

    private async Task<GradeCategory> MapCreateEntityAsync(CreateUpdateGradeCategoryDto createInput)
    {
        await EnsureValidAccessibleBranchAsync(createInput.NurseryBranchId);
        return new GradeCategory(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            createInput.NurseryBranchId,
            createInput.Name,
            createInput.Icon,
            createInput.ColorToken,
            createInput.Description,
            createInput.IsActive);
    }

    protected override async Task MapToEntityAsync(CreateUpdateGradeCategoryDto updateInput, GradeCategory entity)
    {
        await EnsureValidAccessibleBranchAsync(updateInput.NurseryBranchId);
        entity.SetName(updateInput.Name);
        entity.SetNurseryBranch(updateInput.NurseryBranchId);
        entity.SetIcon(updateInput.Icon);
        entity.SetColorToken(updateInput.ColorToken);
        entity.SetDescription(updateInput.Description);
        entity.SetIsActive(updateInput.IsActive);
    }

    protected override GradeCategoryDto MapToGetOutputDto(GradeCategory entity)
    {
        return new GradeCategoryDto
        {
            Id = entity.Id,
            NurseryBranchId = entity.NurseryBranchId,
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

    private async Task EnsureValidAccessibleBranchAsync(Guid nurseryBranchId)
    {
        if (nurseryBranchId == Guid.Empty)
        {
            throw new BusinessException("NurseryHub:GradeCategories:InvalidBranch");
        }

        var branch = await _nurseryBranchRepository.FindAsync(nurseryBranchId);
        if (branch == null || branch.TenantId != CurrentTenant.Id)
        {
            throw new BusinessException("NurseryHub:GradeCategories:InvalidBranch");
        }

        await EnsureCanAccessBranchAsync(nurseryBranchId);
    }
}
