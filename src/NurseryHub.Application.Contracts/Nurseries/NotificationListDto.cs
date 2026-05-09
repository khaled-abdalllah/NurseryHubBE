using System;

namespace NurseryHub.Nurseries;

public class NotificationListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public NotificationType NotificationType { get; set; }
    public NotificationAudienceType AudienceType { get; set; }
    public Guid SentByUserId { get; set; }
    public DateTime? SentDate { get; set; }
    public NotificationStatus Status { get; set; }
    public int TotalRecipients { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public int ReadCount { get; set; }
}
