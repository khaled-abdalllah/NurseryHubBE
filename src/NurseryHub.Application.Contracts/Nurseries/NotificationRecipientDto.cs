using System;

namespace NurseryHub.Nurseries;

public class NotificationRecipientDto
{
    public Guid ParentUserId { get; set; }
    public NotificationDeliveryStatus DeliveryStatus { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? FailureReason { get; set; }
}
