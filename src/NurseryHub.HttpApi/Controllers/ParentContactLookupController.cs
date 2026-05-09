using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryHub.Nurseries;
using NurseryHub.Security;

namespace NurseryHub.Controllers;

/// <summary>
/// Explicit route for parent lookup by phone (avoids relying on ABP conventional controller naming).
/// </summary>
[Area("app")]
[Route("api/app/parent-contact-lookup")]
[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
public class ParentContactLookupController : NurseryHubController
{
    private readonly IParentContactLookupAppService _lookupAppService;

    public ParentContactLookupController(IParentContactLookupAppService lookupAppService)
    {
        _lookupAppService = lookupAppService;
    }

    [HttpGet("find-parent-contacts-by-phone")]
    public virtual Task<List<ParentContactLookupDto>> FindParentContactsByPhoneAsync(
        [FromQuery] string? phoneNumber)
    {
        return _lookupAppService.FindParentContactsByPhoneAsync(phoneNumber ?? string.Empty);
    }
}
