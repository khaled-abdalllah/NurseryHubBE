using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;

namespace NurseryHub.Nurseries;

[Authorize(Roles = NurseryHubRoles.Parent)]
public class ParentPortalAppService : ApplicationService, IParentPortalAppService
{
    private readonly IRepository<ParentStudent, Guid> _parentStudentRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<NurseryClass, Guid> _classRepository;
    private readonly IRepository<GradeCategory, Guid> _gradeRepository;
    private readonly IRepository<DailyFollowupBook, Guid> _followupRepository;
    private readonly IRepository<DailyFollowupSubjectEntry, Guid> _subjectRepository;
    private readonly IRepository<DailyFollowupActivityEntry, Guid> _activityRepository;
    private readonly IRepository<DailyFollowupMealEntry, Guid> _mealRepository;
    private readonly IRepository<Attendance, Guid> _attendanceRepository;
    private readonly IRepository<Notification, Guid> _notificationRepository;
    private readonly IRepository<NotificationRecipient, Guid> _notificationRecipientRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;
    private readonly NotificationDeliveryService _notificationDeliveryService;
    private readonly ITenantRepository _tenantRepository;
    private readonly NurseryMediaOptions _mediaOptions;

    public ParentPortalAppService(
        IRepository<ParentStudent, Guid> parentStudentRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<NurseryClass, Guid> classRepository,
        IRepository<GradeCategory, Guid> gradeRepository,
        IRepository<DailyFollowupBook, Guid> followupRepository,
        IRepository<DailyFollowupSubjectEntry, Guid> subjectRepository,
        IRepository<DailyFollowupActivityEntry, Guid> activityRepository,
        IRepository<DailyFollowupMealEntry, Guid> mealRepository,
        IRepository<Attendance, Guid> attendanceRepository,
        IRepository<Notification, Guid> notificationRepository,
        IRepository<NotificationRecipient, Guid> notificationRecipientRepository,
        IRepository<UserBranch, Guid> userBranchRepository,
        NotificationDeliveryService notificationDeliveryService,
        ITenantRepository tenantRepository,
        IOptions<NurseryMediaOptions> mediaOptions)
    {
        _parentStudentRepository = parentStudentRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _gradeRepository = gradeRepository;
        _followupRepository = followupRepository;
        _subjectRepository = subjectRepository;
        _activityRepository = activityRepository;
        _mealRepository = mealRepository;
        _attendanceRepository = attendanceRepository;
        _notificationRepository = notificationRepository;
        _notificationRecipientRepository = notificationRecipientRepository;
        _userBranchRepository = userBranchRepository;
        _notificationDeliveryService = notificationDeliveryService;
        _tenantRepository = tenantRepository;
        _mediaOptions = mediaOptions.Value;
    }

    public async Task<List<ParentPortalStudentDto>> GetStudentsAsync()
    {
        var parentId = GetCurrentParentUserId();
        var links = await _parentStudentRepository.GetListAsync(x => x.ParentUserId == parentId);
        if (links.Count == 0)
        {
            return new List<ParentPortalStudentDto>();
        }

        var studentIds = links.Select(x => x.StudentId).Distinct().ToList();
        var students = await _studentRepository.GetListAsync(x => studentIds.Contains(x.Id));
        var classes = await _classRepository.GetListAsync(x => x.Id != Guid.Empty);
        var grades = await _gradeRepository.GetListAsync(x => x.Id != Guid.Empty);

        var classMap = classes.ToDictionary(x => x.Id, x => x);
        var gradeMap = grades.ToDictionary(x => x.Id, x => x.Name);

        var tenantName = await GetTenantNameAsync(CurrentTenant.Id);
        return students
            .OrderBy(x => x.FullName)
            .Select(student =>
            {
                classMap.TryGetValue(student.NurseryClassId ?? Guid.Empty, out var classEntity);
                var gradeName = classEntity?.GradeCategoryId.HasValue == true &&
                                gradeMap.TryGetValue(classEntity.GradeCategoryId.Value, out var grade)
                    ? grade
                    : null;

                return new ParentPortalStudentDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    Photo = ResolveStudentImageDisplayUrl(student.ProfileImageFileName, tenantName),
                    ClassName = classEntity?.Name,
                    GradeName = gradeName,
                };
            })
            .ToList();
    }

    public async Task<PagedResultDto<ParentFollowupTimelineItemDto>> GetFollowUpTimelineAsync(GetParentFollowupTimelineInput input)
    {
        await EnsureParentCanAccessStudentAsync(input.StudentId);

        var queryable = await _followupRepository.GetQueryableAsync();
        var filtered = queryable
            .Where(x => x.StudentId == input.StudentId && x.IsVisibleToParent)
            .WhereIf(input.Date.HasValue, x => x.ReportDate == input.Date!.Value)
            .WhereIf(input.Year.HasValue, x => x.ReportDate.Year == input.Year!.Value)
            .WhereIf(input.Month.HasValue, x => x.ReportDate.Month == input.Month!.Value);

        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var reports = await AsyncExecuter.ToListAsync(
            filtered
                .OrderByDescending(x => x.ReportDate)
                .ThenByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var items = new List<ParentFollowupTimelineItemDto>(reports.Count);
        foreach (var report in reports)
        {
            var subjects = await _subjectRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
            var activities = await _activityRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
            var meals = await _mealRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);

            items.Add(new ParentFollowupTimelineItemDto
            {
                Id = report.Id,
                ReportDate = report.ReportDate,
                LearningSummaries = subjects
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new DailyFollowupSubjectDto
                    {
                        SubjectName = x.SubjectName,
                        SubjectIcon = x.SubjectIcon,
                        IsActive = x.IsActive,
                        Notes = x.Notes,
                        SortOrder = x.SortOrder,
                    })
                    .ToList(),
                Activities = activities.Select(x => x.ActivityType).Distinct().OrderBy(x => x).ToList(),
                Meals = meals
                    .Select(x => new DailyFollowupMealDto
                    {
                        MealType = x.MealType,
                        Status = x.Status,
                        Notes = x.Notes,
                    }).OrderBy(x => x.MealType).ToList(),
                SleptToday = report.SleptToday,
                SleepDuration = report.SleepDuration,
                MoodAfterWaking = report.MoodAfterWaking,
                Mood = report.OverallMood,
                TeacherNote = report.TeacherNote,
            });
        }

        return new PagedResultDto<ParentFollowupTimelineItemDto>(totalCount, items);
    }

    public async Task<ParentStudentAttendanceDayDto> GetAttendanceAsync(GetParentStudentAttendanceInput input)
    {
        await EnsureParentCanAccessStudentAsync(input.StudentId);
        var row = await _attendanceRepository.FirstOrDefaultAsync(
            x => x.StudentId == input.StudentId && x.Date == input.Date);
        if (row == null)
        {
            return new ParentStudentAttendanceDayDto
            {
                Date = input.Date,
                HasAttendanceRecord = false,
                Status = null,
            };
        }

        return new ParentStudentAttendanceDayDto
        {
            Date = row.Date,
            HasAttendanceRecord = true,
            Status = row.Status,
            CheckedInAt = row.CheckedInAt,
            CheckedOutAt = row.CheckedOutAt,
        };
    }

    public async Task<ParentFollowupDetailsDto> GetFollowUpDetailsAsync(Guid id)
    {
        var report = await _followupRepository.GetAsync(id);
        if (!report.IsVisibleToParent)
        {
            throw new AbpAuthorizationException("Parent cannot access non-published reports.");
        }

        var student = await EnsureParentCanAccessStudentAsync(report.StudentId);
        var classEntity = student.NurseryClassId.HasValue
            ? await _classRepository.FindAsync(student.NurseryClassId.Value)
            : null;
        var grade = classEntity?.GradeCategoryId.HasValue == true
            ? await _gradeRepository.FindAsync(classEntity.GradeCategoryId.Value)
            : null;

        var subjects = await _subjectRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
        var activities = await _activityRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
        var meals = await _mealRepository.GetListAsync(x => x.DailyFollowupBookId == report.Id);
        var tenantName = await GetTenantNameAsync(CurrentTenant.Id);

        return new ParentFollowupDetailsDto
        {
            Id = report.Id,
            StudentId = student.Id,
            StudentName = student.FullName,
            StudentPhoto = ResolveStudentImageDisplayUrl(student.ProfileImageFileName, tenantName),
            ClassName = classEntity?.Name,
            GradeName = grade?.Name,
            ReportDate = report.ReportDate,
            LearningSummaries = subjects
                .OrderBy(x => x.SortOrder)
                .Select(x => new DailyFollowupSubjectDto
                {
                    SubjectName = x.SubjectName,
                    SubjectIcon = x.SubjectIcon,
                    IsActive = x.IsActive,
                    Notes = x.Notes,
                    SortOrder = x.SortOrder,
                })
                .ToList(),
            Activities = activities.Select(x => x.ActivityType).Distinct().OrderBy(x => x).ToList(),
            Meals = meals
                .Select(x => new DailyFollowupMealDto
                {
                    MealType = x.MealType,
                    Status = x.Status,
                    Notes = x.Notes,
                }).OrderBy(x => x.MealType).ToList(),
            SleptToday = report.SleptToday,
            SleepDuration = report.SleepDuration,
            MoodAfterWaking = report.MoodAfterWaking,
            Mood = report.OverallMood,
            TeacherNote = report.TeacherNote,
        };
    }

    public async Task<List<ParentPortalNotificationDto>> GetMyNotificationsAsync(int maxResultCount = 50)
    {
        var parentUserId = GetCurrentParentUserId();
        if (maxResultCount < 1 || maxResultCount > 200)
        {
            maxResultCount = 50;
        }

        var recipientsQ = await _notificationRecipientRepository.GetQueryableAsync();
        var notificationsQ = await _notificationRepository.GetQueryableAsync();

        var query =
            from r in recipientsQ
            join n in notificationsQ on r.NotificationId equals n.Id
            where r.ParentUserId == parentUserId
                && n.AudienceType != NotificationAudienceType.ParentToNursery
                && n.Status == NotificationStatus.Sent
                && r.DeliveryStatus != NotificationDeliveryStatus.Pending
            orderby n.SentDate descending, r.CreationTime descending
            select new ParentPortalNotificationDto
            {
                RecipientId = r.Id,
                NotificationId = n.Id,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                PriorityLevel = n.PriorityLevel,
                SentDate = n.SentDate,
                DeliveryStatus = r.DeliveryStatus,
                ReadAt = r.ReadAt,
            };

        return await AsyncExecuter.ToListAsync(query.Take(maxResultCount));
    }

    public async Task<int> GetMyUnreadNotificationCountAsync()
    {
        var parentUserId = GetCurrentParentUserId();
        var recipientsQ = await _notificationRecipientRepository.GetQueryableAsync();
        var notificationsQ = await _notificationRepository.GetQueryableAsync();

        var query =
            from r in recipientsQ
            join n in notificationsQ on r.NotificationId equals n.Id
            where r.ParentUserId == parentUserId
                && n.AudienceType != NotificationAudienceType.ParentToNursery
                && n.Status == NotificationStatus.Sent
                && r.DeliveryStatus != NotificationDeliveryStatus.Pending
                && r.ReadAt == null
                && r.DeliveryStatus != NotificationDeliveryStatus.Read
            select r.Id;

        return await AsyncExecuter.CountAsync(query);
    }

    [RemoteService(IsEnabled = false)]
    public async Task MarkMyNotificationReadAsync(Guid recipientId)
    {
        var parentUserId = GetCurrentParentUserId();
        var recipient = await _notificationRecipientRepository.GetAsync(recipientId);
        if (recipient.ParentUserId != parentUserId)
        {
            throw new AbpAuthorizationException("Notification does not belong to the current parent.");
        }

        var notification = await _notificationRepository.GetAsync(recipient.NotificationId);
        if (notification.AudienceType == NotificationAudienceType.ParentToNursery)
        {
            throw new AbpAuthorizationException("Notification is not part of the parent inbox.");
        }

        if (recipient.DeliveryStatus == NotificationDeliveryStatus.Read)
        {
            return;
        }

        recipient.MarkRead(Clock.Now);
        await _notificationRecipientRepository.UpdateAsync(recipient);
    }

    [RemoteService(IsEnabled = false)]
    public async Task SendNotificationToNurseryAsync(SendParentToNurseryNotificationDto input)
    {
        var title = input.Title?.Trim() ?? string.Empty;
        var message = input.Message?.Trim() ?? string.Empty;
        if (title.Length == 0 || message.Length == 0)
        {
            throw new UserFriendlyException(L["NurseryHub:ParentPortal:NotifyNursery:TitleMessageRequired"]);
        }

        if (title.Length > Notification.MaxTitleLength || message.Length > Notification.MaxMessageLength)
        {
            throw new UserFriendlyException(L["NurseryHub:ParentPortal:NotifyNursery:LengthInvalid"]);
        }

        var student = await EnsureParentCanAccessStudentAsync(input.StudentId);
        var branchId = student.NurseryBranchId;
        var parentUserId = GetCurrentParentUserId();

        var staffUserIds = await GetStaffUserIdsForBranchAsync(branchId);
        var notification = new Notification(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            title,
            message,
            NotificationType.ParentMessage,
            PriorityLevel.Normal,
            NotificationAudienceType.ParentToNursery,
            parentUserId,
            branchId);
        notification.SetRelatedStudent(student.Id);

        notification.MarkSending();
        await _notificationRepository.InsertAsync(notification, autoSave: true);

        var deliveredCount = await _notificationDeliveryService.CreateInAppDeliveriesAsync(
            notification.Id,
            notification.TenantId,
            staffUserIds);

        notification.MarkSent(deliveredCount, Clock.Now);
        await _notificationRepository.UpdateAsync(notification);
    }

    public async Task<List<ParentPortalSentToNurseryNotificationDto>> GetMySentToNurseryNotificationsAsync(int maxResultCount = 50)
    {
        var parentUserId = GetCurrentParentUserId();
        if (maxResultCount < 1 || maxResultCount > 200)
        {
            maxResultCount = 50;
        }

        var notificationsQ = await _notificationRepository.GetQueryableAsync();
        var studentsQ = await _studentRepository.GetQueryableAsync();

        var query =
            from n in notificationsQ
            join s in studentsQ on n.RelatedStudentId equals s.Id into studentJoin
            from s in studentJoin.DefaultIfEmpty()
            where n.SentByUserId == parentUserId
                && n.AudienceType == NotificationAudienceType.ParentToNursery
                && n.Status == NotificationStatus.Sent
            orderby n.SentDate descending, n.CreationTime descending
            select new ParentPortalSentToNurseryNotificationDto
            {
                NotificationId = n.Id,
                Title = n.Title,
                Message = n.Message,
                SentDate = n.SentDate,
                StudentName = s != null ? s.FullName : null,
            };

        return await AsyncExecuter.ToListAsync(query.Take(maxResultCount));
    }

    [RemoteService(IsEnabled = false)]
    public async Task<ParentPortalSentToNurseryNotificationDto> GetMySentToNurseryNotificationAsync(Guid notificationId)
    {
        var parentUserId = GetCurrentParentUserId();
        var n = await _notificationRepository.GetAsync(notificationId);
        if (n.SentByUserId != parentUserId || n.AudienceType != NotificationAudienceType.ParentToNursery)
        {
            throw new AbpAuthorizationException("Notification is not part of the parent outbox.");
        }

        string? studentName = null;
        if (n.RelatedStudentId.HasValue)
        {
            var student = await _studentRepository.FindAsync(n.RelatedStudentId.Value);
            studentName = student?.FullName;
        }

        return new ParentPortalSentToNurseryNotificationDto
        {
            NotificationId = n.Id,
            Title = n.Title,
            Message = n.Message,
            SentDate = n.SentDate,
            StudentName = studentName,
        };
    }

    private async Task<List<Guid>> GetStaffUserIdsForBranchAsync(Guid branchId)
    {
        if (!CurrentTenant.Id.HasValue)
        {
            return new List<Guid>();
        }

        var links = await _userBranchRepository.GetListAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.NurseryBranchId == branchId);

        return links.Select(x => x.UserId).Distinct().ToList();
    }

    private Guid GetCurrentParentUserId()
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not authenticated.");
        }

        return CurrentUser.Id.Value;
    }

    private async Task<Student> EnsureParentCanAccessStudentAsync(Guid studentId)
    {
        var parentId = GetCurrentParentUserId();
        var link = await _parentStudentRepository.FirstOrDefaultAsync(
            x => x.ParentUserId == parentId && x.StudentId == studentId);
        if (link == null)
        {
            throw new AbpAuthorizationException("Parent cannot access this student.");
        }

        if (CurrentTenant.Id.HasValue && link.TenantId != CurrentTenant.Id)
        {
            throw new AbpAuthorizationException("Cross-tenant student access is not allowed.");
        }

        var student = await _studentRepository.GetAsync(studentId);
        if (CurrentTenant.Id.HasValue && student.TenantId != CurrentTenant.Id)
        {
            throw new AbpAuthorizationException("Cross-tenant student access is not allowed.");
        }

        return student;
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
