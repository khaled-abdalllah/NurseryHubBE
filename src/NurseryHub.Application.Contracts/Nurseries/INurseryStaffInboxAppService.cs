using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NurseryHub;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryStaffInboxAppService : IApplicationService
{
    Task<List<NurseryStaffInboxNotificationDto>> GetMyInboxNotificationsAsync(int maxResultCount = NurseryHubPagingDefaults.PageSize);

    Task<int> GetMyUnreadInboxNotificationCountAsync();

    [RemoteService(IsEnabled = false)]
    Task MarkMyInboxNotificationReadAsync(Guid recipientId);
}
