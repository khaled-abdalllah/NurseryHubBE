using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
public class StudentApplicationAppService : ApplicationService, IStudentApplicationAppService
{
    private readonly IRepository<StudentApplication, Guid> _repository;
    private readonly IRepository<NurseryBranch, Guid> _nurseryBranchRepository;
    private readonly IRepository<GradeCategory, Guid> _gradeCategoryRepository;
    private readonly IRepository<NurseryClass, Guid> _nurseryClassRepository;
    private readonly IStudentAppService _studentAppService;

    public StudentApplicationAppService(
        IRepository<StudentApplication, Guid> repository,
        IRepository<NurseryBranch, Guid> nurseryBranchRepository,
        IRepository<GradeCategory, Guid> gradeCategoryRepository,
        IRepository<NurseryClass, Guid> nurseryClassRepository,
        IStudentAppService studentAppService)
    {
        _repository = repository;
        _nurseryBranchRepository = nurseryBranchRepository;
        _gradeCategoryRepository = gradeCategoryRepository;
        _nurseryClassRepository = nurseryClassRepository;
        _studentAppService = studentAppService;
    }

    public virtual async Task<StudentApplicationSummaryDto> GetSummaryAsync(Guid nurseryBranchId)
    {
        await ValidateBranchAsync(nurseryBranchId);

        var queryable = await _repository.GetQueryableAsync();
        var filtered = queryable.Where(x => x.NurseryBranchId == nurseryBranchId);

        var total = await AsyncExecuter.CountAsync(filtered);
        var pending = await AsyncExecuter.CountAsync(filtered.Where(x => x.Status == ApplicationStatus.Pending));
        var underReview = await AsyncExecuter.CountAsync(filtered.Where(x => x.Status == ApplicationStatus.UnderReview));
        var accepted = await AsyncExecuter.CountAsync(filtered.Where(x => x.Status == ApplicationStatus.Accepted));
        var rejected = await AsyncExecuter.CountAsync(filtered.Where(x => x.Status == ApplicationStatus.Rejected));

        return new StudentApplicationSummaryDto
        {
            TotalApplications = total,
            PendingApplications = pending,
            UnderReviewApplications = underReview,
            AcceptedApplications = accepted,
            RejectedApplications = rejected,
        };
    }

    public virtual async Task<PagedResultDto<StudentApplicationDto>> GetListAsync(StudentApplicationPagedRequestDto input)
    {
        await ValidateBranchAsync(input.NurseryBranchId);

        var apps = await _repository.GetQueryableAsync();
        var grades = await _gradeCategoryRepository.GetQueryableAsync();

        var filtered = apps.Where(x => x.NurseryBranchId == input.NurseryBranchId)
            .WhereIf(!input.ChildNameFilter.IsNullOrWhiteSpace(),
                x => x.ChildFullName.Contains(input.ChildNameFilter!))
            .WhereIf(!input.ParentPhoneFilter.IsNullOrWhiteSpace(),
                x => x.ParentPhoneNumber.Contains(input.ParentPhoneFilter!))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status!.Value)
            .WhereIf(input.RequestedGradeCategoryId.HasValue,
                x => x.RequestedGradeCategoryId == input.RequestedGradeCategoryId!.Value)
            .WhereIf(input.FromDate.HasValue, x => x.CreationTime >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue,
                x => x.CreationTime <= input.ToDate!.Value.Date.AddDays(1).AddTicks(-1));

        var query =
            from app in filtered
            join g in grades on app.RequestedGradeCategoryId equals g.Id into gj
            from g in gj.DefaultIfEmpty()
            select new { Application = app, GradeName = g != null ? g.Name : null };

        var totalCount = await AsyncExecuter.CountAsync(query);
        var rawItems = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.Application.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var items = rawItems.Select(x => MapToDto(x.Application, x.GradeName)).ToList();

        return new PagedResultDto<StudentApplicationDto>(totalCount, items);
    }

    public virtual async Task<StudentApplicationDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        await ValidateBranchAsync(entity.NurseryBranchId);
        var gradeName = await ResolveGradeNameAsync(entity.RequestedGradeCategoryId);
        return MapToDto(entity, gradeName);
    }

    public virtual async Task<StudentApplicationDto> CreateAsync(CreateUpdateStudentApplicationDto input)
    {
        TrimDto(input);
        await ValidateBranchAsync(input.NurseryBranchId);
        await ValidateGradeCategoryAsync(input.NurseryBranchId, input.RequestedGradeCategoryId);

        var tenantId = await ResolveTenantIdAsync(input.NurseryBranchId);
        var entity = new StudentApplication(
            GuidGenerator.Create(),
            tenantId,
            input.NurseryBranchId,
            input.RequestedGradeCategoryId,
            input.ChildFullName,
            input.BirthDate,
            input.Gender,
            input.ParentFullName,
            input.ParentPhoneNumber,
            input.SecondaryPhoneNumber,
            input.Email,
            input.Notes,
            input.Status);

        await _repository.InsertAsync(entity, autoSave: true);
        var gradeName = await ResolveGradeNameAsync(entity.RequestedGradeCategoryId);
        return MapToDto(entity, gradeName);
    }

    public virtual async Task<StudentApplicationDto> UpdateAsync(Guid id, CreateUpdateStudentApplicationDto input)
    {
        TrimDto(input);
        var entity = await _repository.GetAsync(id);
        await ValidateBranchAsync(entity.NurseryBranchId);
        await ValidateBranchAsync(input.NurseryBranchId);
        await ValidateGradeCategoryAsync(input.NurseryBranchId, input.RequestedGradeCategoryId);

        entity.SetNurseryBranch(input.NurseryBranchId);
        entity.SetRequestedGradeCategory(input.RequestedGradeCategoryId);
        entity.SetChildFullName(input.ChildFullName);
        entity.SetBirthDate(input.BirthDate);
        entity.SetGender(input.Gender);
        entity.SetParentFullName(input.ParentFullName);
        entity.SetParentPhoneNumber(input.ParentPhoneNumber);
        entity.SetSecondaryPhoneNumber(input.SecondaryPhoneNumber);
        entity.SetEmail(input.Email);
        entity.SetNotes(input.Notes);
        entity.SetStatus(input.Status);

        await _repository.UpdateAsync(entity, autoSave: true);
        var gradeName = await ResolveGradeNameAsync(entity.RequestedGradeCategoryId);
        return MapToDto(entity, gradeName);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        await ValidateBranchAsync(entity.NurseryBranchId);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public virtual async Task<StudentApplicationDto> ChangeStatusAsync(Guid id, ChangeStudentApplicationStatusDto input)
    {
        var entity = await _repository.GetAsync(id);
        await ValidateBranchAsync(entity.NurseryBranchId);
        entity.SetStatus(input.Status);
        await _repository.UpdateAsync(entity, autoSave: true);
        var gradeName = await ResolveGradeNameAsync(entity.RequestedGradeCategoryId);
        return MapToDto(entity, gradeName);
    }

    public virtual async Task<StudentDto> ConvertToStudentAsync(Guid id, ConvertStudentApplicationDto input)
    {
        var app = await _repository.GetAsync(id);
        await ValidateBranchAsync(app.NurseryBranchId);

        if (app.Status != ApplicationStatus.Accepted)
        {
            throw new BusinessException("NurseryHub:StudentApplication:MustBeAccepted");
        }

        var nurseryClassId = await ResolveTargetClassIdAsync(app, input.NurseryClassId);
        var enrollmentDate = input.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var createStudent = new CreateUpdateStudentDto
        {
            NurseryBranchId = app.NurseryBranchId,
            NurseryClassId = nurseryClassId,
            FullName = app.ChildFullName,
            BirthDate = app.BirthDate,
            Gender = app.Gender,
            FatherName = app.ParentFullName,
            FatherIdentityNumber = null,
            FatherPhoneNumber = app.ParentPhoneNumber,
            MotherName = string.Empty,
            MotherIdentityNumber = null,
            MotherPhoneNumber = app.SecondaryPhoneNumber ?? string.Empty,
            EmergencyContactNumber = app.ParentPhoneNumber,
            EnrollmentDate = enrollmentDate,
            HealthNotes = app.Notes,
            AttendsSunday = true,
            AttendsMonday = false,
            AttendsTuesday = false,
            AttendsWednesday = false,
            AttendsThursday = false,
            AttendsFriday = false,
            AttendsSaturday = false,
            IsActive = true,
            CreateParentPortalAccount = false,
        };

        var student = await _studentAppService.CreateAsync(createStudent);
        await _repository.DeleteAsync(app, autoSave: true);
        return student;
    }

    private async Task<Guid?> ResolveTargetClassIdAsync(StudentApplication app, Guid? requestedClassId)
    {
        if (requestedClassId.HasValue)
        {
            var cls = await _nurseryClassRepository.FindAsync(requestedClassId.Value);
            if (cls == null || cls.NurseryBranchId != app.NurseryBranchId)
            {
                throw new BusinessException("NurseryHub:StudentApplication:InvalidClass");
            }

            return cls.Id;
        }

        if (!app.RequestedGradeCategoryId.HasValue)
        {
            return null;
        }

        var classes = await _nurseryClassRepository.GetQueryableAsync();
        var match = await AsyncExecuter.FirstOrDefaultAsync(
            classes
                .Where(c =>
                    c.NurseryBranchId == app.NurseryBranchId &&
                    c.IsActive &&
                    c.GradeCategoryId == app.RequestedGradeCategoryId)
                .OrderBy(c => c.Name)
                .Select(c => (Guid?)c.Id));

        return match;
    }

    private async Task<string?> ResolveGradeNameAsync(Guid? gradeCategoryId)
    {
        if (!gradeCategoryId.HasValue)
        {
            return null;
        }

        var g = await _gradeCategoryRepository.FindAsync(gradeCategoryId.Value);
        return g?.Name;
    }

    private async Task ValidateBranchAsync(Guid nurseryBranchId)
    {
        var branch = await _nurseryBranchRepository.FindAsync(nurseryBranchId);
        if (branch == null)
        {
            throw new BusinessException("NurseryHub:StudentApplication:BranchNotFound");
        }

        if (CurrentTenant.Id.HasValue && branch.TenantId != CurrentTenant.Id)
        {
            throw new BusinessException("NurseryHub:StudentApplication:BranchNotFound");
        }
    }

    private async Task ValidateGradeCategoryAsync(Guid nurseryBranchId, Guid? gradeCategoryId)
    {
        if (!gradeCategoryId.HasValue)
        {
            return;
        }

        var grade = await _gradeCategoryRepository.FindAsync(gradeCategoryId.Value);
        if (grade == null || grade.NurseryBranchId != nurseryBranchId)
        {
            throw new BusinessException("NurseryHub:StudentApplication:GradeInvalidForBranch");
        }
    }

    private async Task<Guid> ResolveTenantIdAsync(Guid nurseryBranchId)
    {
        if (CurrentTenant.Id.HasValue)
        {
            return CurrentTenant.Id.Value;
        }

        var branch = await _nurseryBranchRepository.FindAsync(nurseryBranchId);
        if (branch?.TenantId.HasValue == true)
        {
            return branch.TenantId.Value;
        }

        throw new BusinessException("NurseryHub:StudentApplication:TenantIdRequired");
    }

    private static StudentApplicationDto MapToDto(StudentApplication entity, string? gradeName)
    {
        return new StudentApplicationDto
        {
            Id = entity.Id,
            NurseryBranchId = entity.NurseryBranchId,
            RequestedGradeCategoryId = entity.RequestedGradeCategoryId,
            RequestedGradeName = gradeName,
            ChildFullName = entity.ChildFullName,
            BirthDate = entity.BirthDate,
            Gender = entity.Gender,
            ParentFullName = entity.ParentFullName,
            ParentPhoneNumber = entity.ParentPhoneNumber,
            SecondaryPhoneNumber = entity.SecondaryPhoneNumber,
            Email = entity.Email,
            Notes = entity.Notes,
            Status = entity.Status,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
        };
    }

    private static void TrimDto(CreateUpdateStudentApplicationDto input)
    {
        input.ChildFullName = input.ChildFullName.Trim();
        input.Gender = input.Gender.Trim();
        input.ParentFullName = input.ParentFullName.Trim();
        input.ParentPhoneNumber = input.ParentPhoneNumber.Trim();
        input.SecondaryPhoneNumber = string.IsNullOrWhiteSpace(input.SecondaryPhoneNumber)
            ? null
            : input.SecondaryPhoneNumber.Trim();
        input.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        input.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
    }
}
