using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryHub.Nurseries;
using NurseryHub.Security;

namespace NurseryHub.Controllers;

[Area("app")]
[Route("api/app/nursery-staff-inbox")]
[Authorize(Roles =
    $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.BranchManager},{NurseryHubRoles.Teacher},{NurseryHubRoles.Accountant}")]
public class StaffInboxNotificationsController : NurseryHubController
{
    private readonly INurseryStaffInboxAppService _nurseryStaffInboxAppService;

    public StaffInboxNotificationsController(INurseryStaffInboxAppService nurseryStaffInboxAppService)
    {
        _nurseryStaffInboxAppService = nurseryStaffInboxAppService;
    }

    [HttpPost("my-notifications/mark-read")]
    public virtual Task MarkMyInboxNotificationReadAsync([FromQuery] Guid recipientId)
    {
        return _nurseryStaffInboxAppService.MarkMyInboxNotificationReadAsync(recipientId);
    }
}
