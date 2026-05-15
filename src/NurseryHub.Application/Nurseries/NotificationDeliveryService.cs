using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

public class NotificationDeliveryService : ITransientDependency
{
    private readonly IRepository<NotificationRecipient, Guid> _recipientRepository;

    public NotificationDeliveryService(IRepository<NotificationRecipient, Guid> recipientRepository)
    {
        _recipientRepository = recipientRepository;
    }

    /// <summary>
    /// Ensures one in-app row per parent user. Updates pending rows to delivered; inserts new rows when missing.
    /// </summary>
    public async Task<int> CreateInAppDeliveriesAsync(Guid notificationId, Guid? tenantId, IReadOnlyList<Guid> parentUserIds)
    {
        var distinct = parentUserIds.Distinct().ToList();
        if (distinct.Count == 0)
        {
            return 0;
        }

        var existing = await _recipientRepository.GetListAsync(x => x.NotificationId == notificationId);
        var byParent = existing.ToDictionary(x => x.ParentUserId);

        var deliveredAt = DateTime.UtcNow;
        var lastIndex = distinct.Count - 1;
        for (var i = 0; i < distinct.Count; i++)
        {
            var parentUserId = distinct[i];
            var autoSave = i == lastIndex;

            if (byParent.TryGetValue(parentUserId, out var row))
            {
                if (row.DeliveryStatus == NotificationDeliveryStatus.Pending)
                {
                    row.MarkDelivered(deliveredAt);
                    await _recipientRepository.UpdateAsync(row, autoSave);
                }
            }
            else
            {
                var recipient = new NotificationRecipient(Guid.NewGuid(), tenantId, notificationId, parentUserId);
                recipient.MarkDelivered(deliveredAt);
                await _recipientRepository.InsertAsync(recipient, autoSave);
            }
        }

        return distinct.Count;
    }
}
