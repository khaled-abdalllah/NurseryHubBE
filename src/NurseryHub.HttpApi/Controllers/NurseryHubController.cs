using NurseryHub.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace NurseryHub.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class NurseryHubController : AbpControllerBase
{
    protected NurseryHubController()
    {
        LocalizationResource = typeof(NurseryHubResource);
    }
}
