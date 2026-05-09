using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class Notification : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxTitleLength = 256;
    public const int MaxMessageLength = 4000;

    public Guid? TenantId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public NotificationType NotificationType { get; private set; }
    public PriorityLevel PriorityLevel { get; private set; }
    public NotificationAudienceType AudienceType { get; private set; }
    public Guid SentByUserId { get; private set; }
    public Guid BranchId { get; private set; }
    public bool IsScheduled { get; private set; }
    public DateTime? ScheduledDate { get; private set; }
    public NotificationStatus Status { get; private set; }
    public int TotalRecipients { get; private set; }
    public DateTime? SentDate { get; private set; }

    protected Notification()
    {
    }

    public Notification(Guid id, Guid? tenantId, string title, string message, NotificationType notificationType,
        PriorityLevel priorityLevel, NotificationAudienceType audienceType, Guid sentByUserId, Guid branchId) : base(id)
    {
        TenantId = tenantId;
        SetContent(title, message, notificationType, priorityLevel, audienceType);
        SentByUserId = sentByUserId;
        BranchId = branchId;
        Status = NotificationStatus.Draft;
    }

    public void SetContent(string title, string message, NotificationType notificationType,
        PriorityLevel priorityLevel, NotificationAudienceType audienceType)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), MaxTitleLength);
        Message = Check.NotNullOrWhiteSpace(message, nameof(message), MaxMessageLength);
        NotificationType = notificationType;
        PriorityLevel = priorityLevel;
        AudienceType = audienceType;
    }

    public void SetSchedule(DateTime? scheduledDate)
    {
        IsScheduled = scheduledDate.HasValue;
        ScheduledDate = scheduledDate;
        Status = IsScheduled ? NotificationStatus.Scheduled : NotificationStatus.Draft;
    }

    public void MarkSending()
    {
        Status = NotificationStatus.Sending;
    }

    public void MarkSent(int totalRecipients, DateTime sentDate)
    {
        TotalRecipients = totalRecipients;
        SentDate = sentDate;
        Status = NotificationStatus.Sent;
    }

    public void MarkFailed()
    {
        Status = NotificationStatus.Failed;
    }
}
