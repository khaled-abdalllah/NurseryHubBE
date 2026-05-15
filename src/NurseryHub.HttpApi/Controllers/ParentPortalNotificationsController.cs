using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryHub.Nurseries;
using NurseryHub.Security;

namespace NurseryHub.Controllers;

/// <summary>
/// Explicit POST for marking a notification read. ABP conventional routing does not reliably expose POST
/// for <see cref="IParentPortalAppService.MarkMyNotificationReadAsync"/> at the default URL, which caused 405 responses.
/// </summary>
[Area("app")]
[Route("api/app/parent-portal")]
[Authorize(Roles = NurseryHubRoles.Parent)]
public class ParentPortalNotificationsController : NurseryHubController
{
    private readonly IParentPortalAppService _parentPortalAppService;

    public ParentPortalNotificationsController(IParentPortalAppService parentPortalAppService)
    {
        _parentPortalAppService = parentPortalAppService;
    }

    [HttpPost("my-notifications/mark-read")]
    public virtual Task MarkMyNotificationReadAsync([FromQuery] Guid recipientId)
    {
        return _parentPortalAppService.MarkMyNotificationReadAsync(recipientId);
    }

    [HttpPost("notify-nursery")]
    public virtual Task NotifyNurseryAsync([FromBody] SendParentToNurseryNotificationDto input)
    {
        return _parentPortalAppService.SendNotificationToNurseryAsync(input);
    }

    [HttpGet("my-sent-to-nursery/{notificationId}")]
    public virtual Task<ParentPortalSentToNurseryNotificationDto> GetMySentToNurseryNotificationAsync(Guid notificationId)
    {
        return _parentPortalAppService.GetMySentToNurseryNotificationAsync(notificationId);
    }
}
