using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;

namespace NurseryHub.Nurseries;

[Authorize(Roles =
    $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.Teacher},{NurseryHubRoles.BranchManager}")]
public class DailyFollowupBookAppService : ApplicationService, IDailyFollowupBookAppService
{
    private readonly IRepository<DailyFollowupBook, Guid> _dailyFollowupRepository;
    private readonly IRepository<DailyFollowupSubjectEntry, Guid> _subjectRepository;
    private readonly IRepository<DailyFollowupActivityEntry, Guid> _activityRepository;
    private readonly IRepository<DailyFollowupMealEntry, Guid> _mealRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<NurseryClass, Guid> _classRepository;
    private readonly IRepository<NurseryBranch, Guid> _branchRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly NurseryMediaOptions _mediaOptions;

    public DailyFollowupBookAppService(
        IRepository<DailyFollowupBook, Guid> dailyFollowupRepository,
        IRepository<DailyFollowupSubjectEntry, Guid> subjectRepository,
        IRepository<DailyFollowupActivityEntry, Guid> activityRepository,
        IRepository<DailyFollowupMealEntry, Guid> mealRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<NurseryClass, Guid> classRepository,
        IRepository<NurseryBranch, Guid> branchRepository,
        ITenantRepository tenantRepository,
        IOptions<NurseryMediaOptions> mediaOptions)
    {
        _dailyFollowupRepository = dailyFollowupRepository;
        _subjectRepository = subjectRepository;
        _activityRepository = activityRepository;
        _mealRepository = mealRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _branchRepository = branchRepository;
        _tenantRepository = tenantRepository;
        _mediaOptions = mediaOptions.Value;
    }

    public virtual async Task<DailyFollowupBookDto> GetAsync(GetDailyFollowupBookInput input)
    {
        var student = await ValidateStudentInBranchAsync(input.StudentId, input.NurseryBranchId);
        var report = await GetReportAsync(input.StudentId, input.Date);
        return report == null
            ? await CreateEmptyDtoAsync(student, input.Date)
            : await MapToDtoAsync(report, student);
    }

    public virtual async Task<DailyFollowupBookDto> SaveDraftAsync(SaveDailyFollowupBookInput input)
    {
        var report = await SaveInternalAsync(input);
        report.SetDraft();
        await _dailyFollowupRepository.UpdateAsync(report, autoSave: true);

        var student = await _studentRepository.GetAsync(input.StudentId);
        return await MapToDtoAsync(report, student);
    }

    public virtual async Task<DailyFollowupBookDto> SendToParentAsync(SaveDailyFollowupBookInput input)
    {
        var report = await SaveInternalAsync(input);
        report.MarkSentToParent(Clock.Now);
        await _dailyFollowupRepository.UpdateAsync(report, autoSave: true);

        var student = await _studentRepository.GetAsync(input.StudentId);
        return await MapToDtoAsync(report, student);
    }

    private async Task<DailyFollowupBook> SaveInternalAsync(SaveDailyFollowupBookInput input)
    {
        ValidateInput(input);
        await ValidateStudentInBranchAsync(input.StudentId, input.NurseryBranchId);

        var report = await GetReportAsync(input.StudentId, input.ReportDate);
        if (report == null)
        {
            report = new DailyFollowupBook(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                input.StudentId,
                input.NurseryBranchId,
                input.ReportDate,
                input.OverallMood,
                input.TeacherNote,
                input.SleptToday,
                input.SleepDuration,
                input.MoodAfterWaking);
            await _dailyFollowupRepository.InsertAsync(report, autoSave: true);
        }
        else
        {
            if (report.IsVisibleToParent)
            {
                throw new BusinessException("NurseryHub:DailyFollowup:LockedAfterSend");
            }

            report.SetOverallMood(input.OverallMood);
            report.SetTeacherNote(input.TeacherNote);
            report.SetSleep(input.SleptToday, input.SleepDuration, input.MoodAfterWaking);
            await _dailyFollowupRepository.UpdateAsync(report, autoSave: true);
        }

        await ReplaceChildrenAsync(report.Id, input.Subjects, input.Activities, input.Meals);
        return report;
    }

    private async Task ReplaceChildrenAsync(
        Guid reportId,
        IReadOnlyList<DailyFollowupSubjectDto> subjects,
        IReadOnlyList<DailyFollowupActivityType> activities,
        IReadOnlyList<DailyFollowupMealDto> meals)
    {
        var existingSubjects = await _subjectRepository.GetListAsync(x => x.DailyFollowupBookId == reportId);
        var existingActivities = await _activityRepository.GetListAsync(x => x.DailyFollowupBookId == reportId);
        var existingMeals = await _mealRepository.GetListAsync(x => x.DailyFollowupBookId == reportId);

        if (existingSubjects.Count > 0) await _subjectRepository.DeleteManyAsync(existingSubjects, autoSave: true);
        if (existingActivities.Count > 0) await _activityRepository.DeleteManyAsync(existingActivities, autoSave: true);
        if (existingMeals.Count > 0) await _mealRepository.DeleteManyAsync(existingMeals, autoSave: true);

        foreach (var subject in subjects)
        {
            var entity = new DailyFollowupSubjectEntry(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                reportId,
                subject.SubjectName,
                subject.SubjectIcon,
                subject.IsActive,
                subject.Notes,
                subject.SortOrder);
            await _subjectRepository.InsertAsync(entity, autoSave: true);
        }

        foreach (var activity in activities.Distinct())
        {
            var entity = new DailyFollowupActivityEntry(GuidGenerator.Create(), CurrentTenant.Id, reportId, activity);
            await _activityRepository.InsertAsync(entity, autoSave: true);
        }

        foreach (var meal in meals)
        {
            var entity = new DailyFollowupMealEntry(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                reportId,
                meal.MealType,
                meal.Status,
                meal.Notes);
            await _mealRepository.InsertAsync(entity, autoSave: true);
        }
    }

    private async Task<DailyFollowupBook?> GetReportAsync(Guid studentId, DateOnly date)
    {
        return await _dailyFollowupRepository.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ReportDate == date);
    }

    private void ValidateInput(SaveDailyFollowupBookInput input)
    {
        if (input.SleptToday && (string.IsNullOrWhiteSpace(input.SleepDuration) || !input.MoodAfterWaking.HasValue))
        {
            throw new BusinessException("NurseryHub:DailyFollowup:SleepDataRequired");
        }

        var duplicateMeals = input.Meals
            .GroupBy(x => x.MealType)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateMeals.Count > 0)
        {
            throw new BusinessException("NurseryHub:DailyFollowup:DuplicateMealType");
        }
    }

    private async Task<Student> ValidateStudentInBranchAsync(Guid studentId, Guid nurseryBranchId)
    {
        var branch = await _branchRepository.FindAsync(nurseryBranchId);
        if (branch == null)
        {
            throw new BusinessException("NurseryHub:Attendance:BranchNotFound");
        }

        var student = await _studentRepository.FindAsync(studentId);
        if (student == null || student.NurseryBranchId != nurseryBranchId)
        {
            throw new BusinessException("NurseryHub:DailyFollowup:StudentNotInBranch");
        }

        if (!student.IsActive)
        {
            throw new BusinessException("NurseryHub:Student:Inactive");
        }

        return student;
    }

    private async Task<DailyFollowupBookDto> MapToDtoAsync(DailyFollowupBook report, Student student)
    {
        var subjects = await _subjectRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
        var activities = await _activityRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
        var meals = await _mealRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
        var className = await GetClassNameAsync(student.NurseryClassId);
        var tenantName = await GetTenantNameAsync(student.TenantId);

        return new DailyFollowupBookDto
        {
            Id = report.Id,
            StudentId = report.StudentId,
            NurseryBranchId = report.NurseryBranchId,
            StudentName = student.FullName,
            ClassroomName = className,
            StudentProfileImageUrl = ResolveStudentImageDisplayUrl(student.ProfileImageFileName, tenantName),
            ReportDate = report.ReportDate,
            OverallMood = report.OverallMood,
            TeacherNote = report.TeacherNote,
            SleptToday = report.SleptToday,
            SleepDuration = report.SleepDuration,
            MoodAfterWaking = report.MoodAfterWaking,
            IsDraft = report.IsDraft,
            SentToParent = report.SentToParent,
            IsVisibleToParent = report.IsVisibleToParent,
            SentToParentAt = report.SentToParentAt,
            Subjects = subjects
                .OrderBy(x => x.SortOrder)
                .Select(x => new DailyFollowupSubjectDto
                {
                    SubjectName = x.SubjectName,
                    SubjectIcon = x.SubjectIcon,
                    IsActive = x.IsActive,
                    Notes = x.Notes,
                    SortOrder = x.SortOrder,
                }).ToList(),
            Activities = activities.Select(x => x.ActivityType).Distinct().OrderBy(x => x).ToList(),
            Meals = meals
                .Select(x => new DailyFollowupMealDto
                {
                    MealType = x.MealType,
                    Status = x.Status,
                    Notes = x.Notes,
                }).OrderBy(x => x.MealType).ToList(),
        };
    }

    private async Task<DailyFollowupBookDto> CreateEmptyDtoAsync(Student student, DateOnly date)
    {
        var className = await GetClassNameAsync(student.NurseryClassId);
        var tenantName = await GetTenantNameAsync(student.TenantId);

        return new DailyFollowupBookDto
        {
            Id = Guid.Empty,
            StudentId = student.Id,
            NurseryBranchId = student.NurseryBranchId,
            StudentName = student.FullName,
            ClassroomName = className,
            StudentProfileImageUrl = ResolveStudentImageDisplayUrl(student.ProfileImageFileName, tenantName),
            ReportDate = date,
            OverallMood = DailyFollowupMood.Happy,
            TeacherNote = string.Empty,
            SleptToday = false,
            IsDraft = true,
            SentToParent = false,
            IsVisibleToParent = false,
        };
    }

    private async Task<string?> GetClassNameAsync(Guid? classId)
    {
        if (!classId.HasValue)
        {
            return null;
        }

        var nurseryClass = await _classRepository.FindAsync(classId.Value);
        return nurseryClass?.Name;
    }

    private async Task<string?> GetTenantNameAsync(Guid? tenantId)
    {
        if (!tenantId.HasValue)
        {
            return CurrentTenant.Name;
        }

        var tenant = await _tenantRepository.FindAsync(tenantId.Value);
        return tenant?.Name ?? CurrentTenant.Name;
    }

    private string? ResolveStudentImageDisplayUrl(string? fileName, string? tenantName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var safeTenantName = string.IsNullOrWhiteSpace(tenantName) ? "host" : tenantName;
        var baseUrl = _mediaOptions.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{safeTenantName}/students/{fileName}";
    }
}
