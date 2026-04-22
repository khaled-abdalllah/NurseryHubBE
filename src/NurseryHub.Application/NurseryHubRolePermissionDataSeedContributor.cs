using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NurseryHub.Permissions;
using NurseryHub.Security;
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
    private readonly IdentityRoleManager _roleManager;

    public NurseryHubRolePermissionDataSeedContributor(
        IPermissionDataSeeder permissionDataSeeder,
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager)
    {
        _permissionDataSeeder = permissionDataSeeder;
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        foreach (var roleName in new[]
                 {
                     NurseryHubRoles.Admin,
                     NurseryHubRoles.NurseryAdmin,
                     NurseryHubRoles.Student,
                     NurseryHubRoles.Parent,
                 })
        {
            await EnsureRoleExistsAsync(roleName, context.TenantId);
        }

        var roles = await _roleRepository.GetListAsync();
        var adminRole = roles.FirstOrDefault(r =>
            r.Name == NurseryHubRoles.Admin || r.NormalizedName == NurseryHubRoles.Admin.ToUpperInvariant());

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
            NurseryHubPermissions.NurseryBranches.Default,
            NurseryHubPermissions.NurseryBranches.Create,
            NurseryHubPermissions.NurseryBranches.Edit,
            NurseryHubPermissions.NurseryBranches.Delete,
        };

        await _permissionDataSeeder.SeedAsync(
            "R",
            adminRole.Id.ToString(),
            permissions,
            context.TenantId);
    }

    private async Task EnsureRoleExistsAsync(string roleName, Guid? tenantId)
    {
        var normalized = roleName.ToUpperInvariant();
        var existingRole = await _roleRepository.FindByNormalizedNameAsync(normalized);
        if (existingRole != null)
        {
            return;
        }

        var newRole = new IdentityRole(Guid.NewGuid(), roleName, tenantId);
        var result = await _roleManager.CreateAsync(newRole);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
        }
    }
}
