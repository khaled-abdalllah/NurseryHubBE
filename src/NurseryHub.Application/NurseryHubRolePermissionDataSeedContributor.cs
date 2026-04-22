using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NurseryHub.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace NurseryHub;

/// <summary>
/// Grants NurseryHub permissions to the default <c>admin</c> role (host and tenants).
/// Provider key must be the role <see cref="IdentityRole.Id"/> (not the name), or permission checks stay false → 403.
/// </summary>
public class NurseryHubRolePermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IIdentityRoleRepository _roleRepository;

    public NurseryHubRolePermissionDataSeedContributor(
        IPermissionDataSeeder permissionDataSeeder,
        IIdentityRoleRepository roleRepository)
    {
        _permissionDataSeeder = permissionDataSeeder;
        _roleRepository = roleRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var roles = await _roleRepository.GetListAsync();
        var adminRole = roles.FirstOrDefault(r => r.Name == "admin" || r.NormalizedName == "ADMIN");

        if (adminRole == null)
        {
            return;
        }

        var permissions = new List<string>
        {
            NurseryHubPermissions.Nurseries.Default,
            NurseryHubPermissions.Nurseries.Create,
            NurseryHubPermissions.Nurseries.Edit,
            NurseryHubPermissions.Nurseries.Delete,
            NurseryHubPermissions.Governorates.Default,
            NurseryHubPermissions.Governorates.Create,
            NurseryHubPermissions.Governorates.Edit,
            NurseryHubPermissions.Governorates.Delete,
            NurseryHubPermissions.Cities.Default,
            NurseryHubPermissions.Cities.Create,
            NurseryHubPermissions.Cities.Edit,
            NurseryHubPermissions.Cities.Delete,
        };

        await _permissionDataSeeder.SeedAsync(
            "R",
            adminRole.Id.ToString(),
            permissions,
            context.TenantId);
    }
}
