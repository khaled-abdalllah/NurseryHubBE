using Microsoft.Extensions.Localization;
using NurseryHub.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace NurseryHub;

[Dependency(ReplaceServices = true)]
public class NurseryHubBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<NurseryHubResource> _localizer;

    public NurseryHubBrandingProvider(IStringLocalizer<NurseryHubResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
