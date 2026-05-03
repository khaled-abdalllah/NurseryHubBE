using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

[Authorize(Roles = NurseryHubRoles.Admin)]
public class NurseryUserAppService : ApplicationService, INurseryUserAppService
{
    private const string DefaultNurseryUserPassword = "Aa@12345678";

    private static readonly string[] AllowedRoles =
    [
        NurseryHubRoles.NurseryAdmin,
        NurseryHubRoles.Teacher,
        NurseryHubRoles.BranchManager,
    ];

    private readonly IRepository<Nursery, Guid> _nurseryRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IIdentityRoleRepository _identityRoleRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IDataFilter _dataFilter;

    public NurseryUserAppService(
        IRepository<Nursery, Guid> nurseryRepository,
        IIdentityUserRepository identityUserRepository,
        IIdentityRoleRepository identityRoleRepository,
        IdentityUserManager identityUserManager,
        IdentityRoleManager identityRoleManager,
        IDataFilter dataFilter)
    {
        _nurseryRepository = nurseryRepository;
        _identityUserRepository = identityUserRepository;
        _identityRoleRepository = identityRoleRepository;
        _identityUserManager = identityUserManager;
        _identityRoleManager = identityRoleManager;
        _dataFilter = dataFilter;
    }

    public async Task<List<NurseryUserDto>> GetListAsync(Guid nurseryId)
    {
        var nursery = await GetNurseryOrThrowAsync(nurseryId);
        if (!nursery.TenantId.HasValue)
        {
            throw new UserFriendlyException("Nursery tenant is not configured.");
        }

        using (CurrentTenant.Change(nursery.TenantId.Value))
        {
            var users = await _identityUserRepository.GetListAsync();
            users = users.OrderBy(x => x.UserName).ToList();

            var result = new List<NurseryUserDto>(users.Count);
            foreach (var user in users)
            {
                var roles = await _identityUserManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault(r => AllowedRoles.Contains(r)) ?? string.Empty;

                result.Add(new NurseryUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    CreationTime = user.CreationTime,
                });
            }

            return result;
        }
    }

    public async Task<NurseryUserDto> CreateAsync(Guid nurseryId, CreateNurseryUserDto input)
    {
        ValidateRole(input.Role);

        var nursery = await GetNurseryOrThrowAsync(nurseryId);
        if (!nursery.TenantId.HasValue)
        {
            throw new UserFriendlyException("Nursery tenant is not configured.");
        }

        using (CurrentTenant.Change(nursery.TenantId.Value))
        {
            await EnsureRoleExistsAsync(input.Role);

            var user = new IdentityUser(
                GuidGenerator.Create(),
                input.UserName,
                input.Email,
                CurrentTenant.Id);

            var createResult = await _identityUserManager.CreateAsync(user, DefaultNurseryUserPassword);
            ThrowIfFailed(createResult);

            var roleResult = await _identityUserManager.AddToRoleAsync(user, input.Role);
            ThrowIfFailed(roleResult);

            return new NurseryUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = input.Role,
                CreationTime = user.CreationTime,
            };
        }
    }

    public async Task<NurseryUserDto> UpdateAsync(Guid nurseryId, Guid userId, UpdateNurseryUserDto input)
    {
        ValidateRole(input.Role);

        var nursery = await GetNurseryOrThrowAsync(nurseryId);
        if (!nursery.TenantId.HasValue)
        {
            throw new UserFriendlyException("Nursery tenant is not configured.");
        }

        using (CurrentTenant.Change(nursery.TenantId.Value))
        {
            await EnsureRoleExistsAsync(input.Role);

            var user = await _identityUserRepository.GetAsync(userId);
            var setUserNameResult = await _identityUserManager.SetUserNameAsync(user, input.UserName);
            ThrowIfFailed(setUserNameResult);

            var setEmailResult = await _identityUserManager.SetEmailAsync(user, input.Email);
            ThrowIfFailed(setEmailResult);

            var updateResult = await _identityUserManager.UpdateAsync(user);
            ThrowIfFailed(updateResult);

            var currentRoles = await _identityUserManager.GetRolesAsync(user);
            foreach (var role in currentRoles.Where(r => AllowedRoles.Contains(r)))
            {
                var removeResult = await _identityUserManager.RemoveFromRoleAsync(user, role);
                ThrowIfFailed(removeResult);
            }

            var addRoleResult = await _identityUserManager.AddToRoleAsync(user, input.Role);
            ThrowIfFailed(addRoleResult);

            return new NurseryUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = input.Role,
                CreationTime = user.CreationTime,
            };
        }
    }

    private async Task<Nursery> GetNurseryOrThrowAsync(Guid nurseryId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var nursery = await _nurseryRepository.FindAsync(nurseryId);
            if (nursery == null)
            {
                throw new UserFriendlyException("The selected nursery does not exist.");
            }

            return nursery;
        }
    }

    private static void ValidateRole(string role)
    {
        if (!AllowedRoles.Contains(role))
        {
            throw new UserFriendlyException("Invalid role.");
        }
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        var normalizedName = roleName.ToUpperInvariant();
        var role = await _identityRoleRepository.FindByNormalizedNameAsync(normalizedName);
        if (role != null)
        {
            return;
        }

        role = new IdentityRole(GuidGenerator.Create(), roleName, CurrentTenant.Id);
        var createRoleResult = await _identityRoleManager.CreateAsync(role);
        ThrowIfFailed(createRoleResult);
    }

    private static void ThrowIfFailed(Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new UserFriendlyException(errors);
    }
}
