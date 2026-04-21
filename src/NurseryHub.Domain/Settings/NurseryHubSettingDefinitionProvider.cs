using Volo.Abp.Settings;

namespace NurseryHub.Settings;

public class NurseryHubSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(NurseryHubSettings.MySetting1));
    }
}
