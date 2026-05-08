using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Localization;
using NurseryHub.Permissions;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
public class NurseryClassAppService
    : CrudAppService<
            NurseryClass,
            NurseryClassDto,
            Guid,
            GetNurseryClassesInput,
            CreateUpdateNurseryClassDto,
            CreateUpdateNurseryClassDto>,
        INurseryClassAppService
{
    private readonly IRepository<GradeCategory, Guid> _gradeCategoryRepository;
    private readonly IRepository<NurseryBranch, Guid> _nurseryBranchRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IDataFilter _dataFilter;

    public NurseryClassAppService(
        IRepository<NurseryClass, Guid> repository,
        IRepository<GradeCategory, Guid> gradeCategoryRepository,
        IRepository<NurseryBranch, Guid> nurseryBranchRepository,
        IRepository<Student, Guid> studentRepository,
        IDataFilter dataFilter)
        : base(repository)
    {
        _gradeCategoryRepository = gradeCategoryRepository;
        _nurseryBranchRepository = nurseryBranchRepository;
        _studentRepository = studentRepository;
        _dataFilter = dataFilter;
        LocalizationResource = typeof(NurseryHubResource);
    }

    
    public override async Task<PagedResultDto<NurseryClassDto>> GetListAsync(GetNurseryClassesInput input)
    {
        await CheckGetListPolicyAsync();

        if (input.NurseryBranchId == Guid.Empty)
        {
            return new PagedResultDto<NurseryClassDto>(0, new List<NurseryClassDto>());
        }

        var classes = await Repository.GetQueryableAsync();
        var grades = await _gradeCategoryRepository.GetQueryableAsync();
        var students = await _studentRepository.GetQueryableAsync();

        var filtered = classes
            .Where(c => c.NurseryBranchId == input.NurseryBranchId)
            .WhereIf(input.GradeCategoryId.HasValue, c => c.GradeCategoryId == input.GradeCategoryId!.Value)
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                c => c.Name.Contains(input.Filter!) || c.Description.Contains(input.Filter!));

        filtered = ApplySortingToClasses(filtered, input.Sorting);

        var query =
            from c in filtered
            join g in grades on c.GradeCategoryId equals g.Id into gradeJoin
            from g in gradeJoin.DefaultIfEmpty()
            let studentCount = students.Count(s => s.NurseryClassId == c.Id)
            select new NurseryClassDto
            {
                Id = c.Id,
                NurseryBranchId = c.NurseryBranchId,
                GradeCategoryId = c.GradeCategoryId,
                GradeCategoryName = g != null ? g.Name : null,
                GradeCategoryColorToken = g != null ? g.ColorToken : null,
                Name = c.Name,
                Description = c.Description,
                Capacity = c.Capacity,
                MinAgeInMonths = c.MinAgeInMonths,
                MaxAgeInMonths = c.MaxAgeInMonths,
                IsActive = c.IsActive,
                StudentCount = studentCount,
                EnrollmentPercent = c.Capacity > 0 ? studentCount * 100 / c.Capacity : 0,
                CreationTime = c.CreationTime,
                CreatorId = c.CreatorId,
                LastModificationTime = c.LastModificationTime,
                LastModifierId = c.LastModifierId,
                IsDeleted = c.IsDeleted,
                DeleterId = c.DeleterId,
                DeletionTime = c.DeletionTime,
            };

        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<NurseryClassDto>(totalCount, items);
    }

    public override async Task<NurseryClassDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var entity = await GetEntityByIdAsync(id);
        return await MapToDtoAsync(entity);
    }

    public override async Task<NurseryClassDto> CreateAsync(CreateUpdateNurseryClassDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateReferencesAsync(input.NurseryBranchId, input.GradeCategoryId);
        var entity = await MapToEntityAsync(input);
        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToDtoAsync(entity);
    }

    public override async Task<NurseryClassDto> UpdateAsync(Guid id, CreateUpdateNurseryClassDto input)
    {
        await CheckUpdatePolicyAsync();
        await ValidateReferencesAsync(input.NurseryBranchId, input.GradeCategoryId);
        var entity = await GetEntityByIdAsync(id);
        if (entity.NurseryBranchId != input.NurseryBranchId)
        {
            throw new UserFriendlyException(L["NurseryHub:NurseryClasses:BranchMismatch"]);
        }

        await MapToEntityAsync(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToDtoAsync(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);
        var studentCount = await _studentRepository.CountAsync(s => s.NurseryClassId == id);
        if (studentCount > 0)
        {
            throw new UserFriendlyException(L["NurseryHub:NurseryClasses:CannotDeleteHasStudents", entity.Name]);
        }

        await base.DeleteAsync(id);
    }

    protected override async Task<NurseryClass> MapToEntityAsync(CreateUpdateNurseryClassDto createInput)
    {
        return await Task.FromResult(
            new NurseryClass(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                createInput.NurseryBranchId,
                createInput.GradeCategoryId,
                createInput.Name,
                createInput.Description,
                createInput.Capacity,
                createInput.MinAgeInMonths,
                createInput.MaxAgeInMonths,
                createInput.IsActive));
    }

    protected override async Task MapToEntityAsync(CreateUpdateNurseryClassDto updateInput, NurseryClass entity)
    {
        entity.SetName(updateInput.Name);
        entity.SetDescription(updateInput.Description);
        entity.SetGradeCategory(updateInput.GradeCategoryId);
        entity.SetCapacity(updateInput.Capacity);
        entity.SetAgeRange(updateInput.MinAgeInMonths, updateInput.MaxAgeInMonths);
        entity.SetIsActive(updateInput.IsActive);
        await Task.CompletedTask;
    }

    private async Task ValidateReferencesAsync(Guid nurseryBranchId, Guid gradeCategoryId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var branch = await _nurseryBranchRepository.FindAsync(nurseryBranchId);
            if (branch == null)
            {
                throw new UserFriendlyException(L["NurseryHub:NurseryClasses:InvalidBranch"]);
            }

            if (CurrentTenant.Id.HasValue && branch.TenantId != CurrentTenant.Id)
            {
                throw new UserFriendlyException(L["NurseryHub:NurseryClasses:InvalidBranch"]);
            }
        }

        if (!await _gradeCategoryRepository.AnyAsync(g =>
                g.Id == gradeCategoryId &&
                g.NurseryBranchId == nurseryBranchId))
        {
            throw new UserFriendlyException(L["NurseryHub:NurseryClasses:InvalidGrade"]);
        }
    }

    private async Task<NurseryClassDto> MapToDtoAsync(NurseryClass entity)
    {
        var studentCount = await _studentRepository.CountAsync(s => s.NurseryClassId == entity.Id);
        string? gradeName = null;
        string? colorToken = null;
        if (entity.GradeCategoryId.HasValue)
        {
            var grade = await _gradeCategoryRepository.FindAsync(entity.GradeCategoryId.Value);
            gradeName = grade?.Name;
            colorToken = grade?.ColorToken;
        }

        return new NurseryClassDto
        {
            Id = entity.Id,
            NurseryBranchId = entity.NurseryBranchId,
            GradeCategoryId = entity.GradeCategoryId,
            GradeCategoryName = gradeName,
            GradeCategoryColorToken = colorToken,
            Name = entity.Name,
            Description = entity.Description,
            Capacity = entity.Capacity,
            MinAgeInMonths = entity.MinAgeInMonths,
            MaxAgeInMonths = entity.MaxAgeInMonths,
            IsActive = entity.IsActive,
            StudentCount = studentCount,
            EnrollmentPercent = entity.Capacity > 0 ? studentCount * 100 / entity.Capacity : 0,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime,
        };
    }

    private static IQueryable<NurseryClass> ApplySortingToClasses(IQueryable<NurseryClass> query, string? sorting)
    {
        if (sorting.IsNullOrWhiteSpace())
        {
            return query.OrderByDescending(c => c.CreationTime);
        }

        if (sorting.Contains("name", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderBy(c => c.Name);
        }

        if (sorting.Contains("creationTime", StringComparison.OrdinalIgnoreCase) &&
            sorting.Contains("desc", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderByDescending(c => c.CreationTime);
        }

        if (sorting.Contains("creationTime", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderBy(c => c.CreationTime);
        }

        return query.OrderByDescending(c => c.CreationTime);
    }
}
