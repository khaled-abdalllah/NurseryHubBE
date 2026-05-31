using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NurseryHub;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IParentPortalAppService : IApplicationService
{
    Task<List<ParentPortalStudentDto>> GetStudentsAsync();

    Task<PagedResultDto<ParentFollowupTimelineItemDto>> GetFollowUpTimelineAsync(GetParentFollowupTimelineInput input);

    Task<ParentFollowupDetailsDto> GetFollowUpDetailsAsync(Guid id);

    Task<ParentStudentAttendanceDayDto> GetAttendanceAsync(GetParentStudentAttendanceInput input);

    Task<List<ParentPortalNotificationDto>> GetMyNotificationsAsync(int maxResultCount = NurseryHubPagingDefaults.PageSize);

    Task<List<ParentPortalSentToNurseryNotificationDto>> GetMySentToNurseryNotificationsAsync(int maxResultCount = NurseryHubPagingDefaults.PageSize);

    /// <summary>HTTP route: use <c>ParentPortalNotificationsController</c> (GET .../my-sent-to-nursery/{notificationId}).</summary>
    [RemoteService(IsEnabled = false)]
    Task<ParentPortalSentToNurseryNotificationDto> GetMySentToNurseryNotificationAsync(Guid notificationId);

    /// <summary>Unread inbox count for the current parent (lightweight; same visibility rules as <see cref="GetMyNotificationsAsync"/>).</summary>
    Task<int> GetMyUnreadNotificationCountAsync();

    /// <summary>HTTP route: use <c>ParentPortalNotificationsController</c> (POST .../my-notifications/mark-read).</summary>
    [RemoteService(IsEnabled = false)]
    Task MarkMyNotificationReadAsync(Guid recipientId);

    /// <summary>HTTP route: use <c>ParentPortalNotificationsController</c> (POST .../notify-nursery).</summary>
    [RemoteService(IsEnabled = false)]
    Task SendNotificationToNurseryAsync(SendParentToNurseryNotificationDto input);
}
