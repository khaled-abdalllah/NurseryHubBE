using NurseryHub.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Permissions;

public class NurseryHubPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(NurseryHubPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(NurseryHubPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NurseryHubResource>(name);
    }
}
