using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryStaffInboxAppService : IApplicationService
{
    Task<List<NurseryStaffInboxNotificationDto>> GetMyInboxNotificationsAsync(int maxResultCount = 50);

    Task<int> GetMyUnreadInboxNotificationCountAsync();

    [RemoteService(IsEnabled = false)]
    Task MarkMyInboxNotificationReadAsync(Guid recipientId);
}
