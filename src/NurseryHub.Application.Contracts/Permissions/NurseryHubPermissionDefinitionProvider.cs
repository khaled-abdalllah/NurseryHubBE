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

        var nurseries = myGroup.AddPermission(NurseryHubPermissions.Nurseries.Default, L("Permission:Nurseries"));
        nurseries.AddChild(NurseryHubPermissions.Nurseries.Create, L("Permission:Nurseries.Create"));
        nurseries.AddChild(NurseryHubPermissions.Nurseries.Edit, L("Permission:Nurseries.Edit"));
        nurseries.AddChild(NurseryHubPermissions.Nurseries.Delete, L("Permission:Nurseries.Delete"));

        var governorates = myGroup.AddPermission(NurseryHubPermissions.Governorates.Default, L("Permission:Governorates"));
        governorates.AddChild(NurseryHubPermissions.Governorates.Create, L("Permission:Governorates.Create"));
        governorates.AddChild(NurseryHubPermissions.Governorates.Edit, L("Permission:Governorates.Edit"));
        governorates.AddChild(NurseryHubPermissions.Governorates.Delete, L("Permission:Governorates.Delete"));

        var cities = myGroup.AddPermission(NurseryHubPermissions.Cities.Default, L("Permission:Cities"));
        cities.AddChild(NurseryHubPermissions.Cities.Create, L("Permission:Cities.Create"));
        cities.AddChild(NurseryHubPermissions.Cities.Edit, L("Permission:Cities.Edit"));
        cities.AddChild(NurseryHubPermissions.Cities.Delete, L("Permission:Cities.Delete"));

        var nurseryBranches = myGroup.AddPermission(NurseryHubPermissions.NurseryBranches.Default, L("Permission:NurseryBranches"));
        nurseryBranches.AddChild(NurseryHubPermissions.NurseryBranches.Create, L("Permission:NurseryBranches.Create"));
        nurseryBranches.AddChild(NurseryHubPermissions.NurseryBranches.Edit, L("Permission:NurseryBranches.Edit"));
        nurseryBranches.AddChild(NurseryHubPermissions.NurseryBranches.Delete, L("Permission:NurseryBranches.Delete"));

        var students = myGroup.AddPermission(NurseryHubPermissions.Students.Default, L("Permission:Students"));
        students.AddChild(NurseryHubPermissions.Students.Create, L("Permission:Students.Create"));
        students.AddChild(NurseryHubPermissions.Students.Edit, L("Permission:Students.Edit"));
        students.AddChild(NurseryHubPermissions.Students.Delete, L("Permission:Students.Delete"));

        var gradeCategories = myGroup.AddPermission(NurseryHubPermissions.GradeCategories.Default, L("Permission:GradeCategories"));
        gradeCategories.AddChild(NurseryHubPermissions.GradeCategories.Create, L("Permission:GradeCategories.Create"));
        gradeCategories.AddChild(NurseryHubPermissions.GradeCategories.Edit, L("Permission:GradeCategories.Edit"));
        gradeCategories.AddChild(NurseryHubPermissions.GradeCategories.Delete, L("Permission:GradeCategories.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NurseryHubResource>(name);
    }
}
