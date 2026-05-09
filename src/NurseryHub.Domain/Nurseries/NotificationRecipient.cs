using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class NotificationRecipient : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxFailureReasonLength = 512;

    public Guid? TenantId { get; private set; }
    public Guid NotificationId { get; private set; }
    public Guid ParentUserId { get; private set; }
    public NotificationDeliveryStatus DeliveryStatus { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? FailureReason { get; private set; }

    protected NotificationRecipient()
    {
    }

    public NotificationRecipient(Guid id, Guid? tenantId, Guid notificationId, Guid parentUserId) : base(id)
    {
        TenantId = tenantId;
        NotificationId = notificationId;
        ParentUserId = parentUserId;
        DeliveryStatus = NotificationDeliveryStatus.Pending;
    }

    public void MarkDelivered(DateTime deliveredAt)
    {
        DeliveryStatus = NotificationDeliveryStatus.Delivered;
        DeliveredAt = deliveredAt;
        FailureReason = null;
    }

    public void MarkFailed(string? reason)
    {
        DeliveryStatus = NotificationDeliveryStatus.Failed;
        FailureReason = reason?.Length > MaxFailureReasonLength ? reason[..MaxFailureReasonLength] : reason;
    }

    public void MarkRead(DateTime readAt)
    {
        ReadAt = readAt;
        DeliveryStatus = NotificationDeliveryStatus.Read;
    }
}
