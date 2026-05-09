using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DependencyInjection;

namespace NurseryHub.Nurseries;

public class NotificationDeliveryService : ITransientDependency
{
    private readonly IRepository<NotificationRecipient, Guid> _recipientRepository;

    public NotificationDeliveryService(IRepository<NotificationRecipient, Guid> recipientRepository)
    {
        _recipientRepository = recipientRepository;
    }

    public async Task<int> CreateInAppDeliveriesAsync(Guid notificationId, Guid? tenantId, IReadOnlyList<Guid> parentIds)
    {
        foreach (var parentId in parentIds)
        {
            var recipient = new NotificationRecipient(Guid.NewGuid(), tenantId, notificationId, parentId);
            recipient.MarkDelivered(DateTime.UtcNow);
            await _recipientRepository.InsertAsync(recipient, autoSave: false);
        }

        return parentIds.Count;
    }
}
