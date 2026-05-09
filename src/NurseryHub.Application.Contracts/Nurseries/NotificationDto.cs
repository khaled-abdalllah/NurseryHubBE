using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class NotificationDto : FullAuditedEntityDto<Guid>
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationType NotificationType { get; set; }
    public PriorityLevel PriorityLevel { get; set; }
    public NotificationAudienceType AudienceType { get; set; }
    public Guid SentByUserId { get; set; }
    public Guid BranchId { get; set; }
    public bool IsScheduled { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public NotificationStatus Status { get; set; }
    public int TotalRecipients { get; set; }
    public DateTime? SentDate { get; set; }
}
