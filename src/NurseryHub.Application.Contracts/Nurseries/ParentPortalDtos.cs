using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class ParentPortalStudentDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public string? ClassName { get; set; }
    public string? GradeName { get; set; }
}

public class GetParentFollowupTimelineInput : PagedResultRequestDto
{
    public Guid StudentId { get; set; }
    public DateOnly? Date { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
}

public class GetParentStudentAttendanceInput
{
    public Guid StudentId { get; set; }
    public DateOnly Date { get; set; }
}

public class ParentFollowupTimelineItemDto : EntityDto<Guid>
{
    public DateOnly ReportDate { get; set; }
    public List<DailyFollowupSubjectDto> LearningSummaries { get; set; } = new();
    public List<DailyFollowupActivityType> Activities { get; set; } = new();
    public List<DailyFollowupMealDto> Meals { get; set; } = new();
    public bool SleptToday { get; set; }
    public string? SleepDuration { get; set; }
    public DailyFollowupNapMood? MoodAfterWaking { get; set; }
    public DailyFollowupMood Mood { get; set; }
    public string TeacherNote { get; set; } = string.Empty;
}

public class ParentFollowupDetailsDto : ParentFollowupTimelineItemDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentPhoto { get; set; }
    public string? ClassName { get; set; }
    public string? GradeName { get; set; }
}

public class ParentStudentAttendanceDayDto
{
    public DateOnly Date { get; set; }
    public bool HasAttendanceRecord { get; set; }
    public AttendanceStatus? Status { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}

public class ParentPortalNotificationDto
{
    public Guid RecipientId { get; set; }
    public Guid NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public PriorityLevel PriorityLevel { get; set; }
    public DateTime? SentDate { get; set; }
    public NotificationDeliveryStatus DeliveryStatus { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class ParentPortalSentToNurseryNotificationDto
{
    public Guid NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? SentDate { get; set; }
    public string? StudentName { get; set; }
}

public class SendParentToNurseryNotificationDto
{
    public Guid StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class NurseryStaffInboxNotificationDto
{
    public Guid RecipientId { get; set; }
    public Guid NotificationId { get; set; }
    public Guid SentByUserId { get; set; }
    public string? StudentName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public PriorityLevel PriorityLevel { get; set; }
    public DateTime? SentDate { get; set; }
    public NotificationDeliveryStatus DeliveryStatus { get; set; }
    public DateTime? ReadAt { get; set; }
}
