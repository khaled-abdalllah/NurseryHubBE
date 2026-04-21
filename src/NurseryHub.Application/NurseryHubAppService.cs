using NurseryHub.Localization;
using Volo.Abp.Application.Services;

namespace NurseryHub;

/* Inherit your application services from this class.
 */
public abstract class NurseryHubAppService : ApplicationService
{
    protected NurseryHubAppService()
    {
        LocalizationResource = typeof(NurseryHubResource);
    }
}
