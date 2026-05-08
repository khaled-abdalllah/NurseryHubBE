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
    private readonly IRepository<NurseryBranch, Guid> _nurseryBranchRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IIdentityRoleRepository _identityRoleRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IDataFilter _dataFilter;

    public NurseryUserAppService(
        IRepository<Nursery, Guid> nurseryRepository,
        IRepository<NurseryBranch, Guid> nurseryBranchRepository,
        IRepository<UserBranch, Guid> userBranchRepository,
        IIdentityUserRepository identityUserRepository,
        IIdentityRoleRepository identityRoleRepository,
        IdentityUserManager identityUserManager,
        IdentityRoleManager identityRoleManager,
        IDataFilter dataFilter)
    {
        _nurseryRepository = nurseryRepository;
        _nurseryBranchRepository = nurseryBranchRepository;
        _userBranchRepository = userBranchRepository;
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
            var userIds = users.Select(x => x.Id).ToList();

            Dictionary<Guid, List<Guid>> byUser;
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var ubQuery = await _userBranchRepository.GetQueryableAsync();
                var bQuery = await _nurseryBranchRepository.GetQueryableAsync();
                var pairs = await AsyncExecuter.ToListAsync(
                    from ub in ubQuery
                    join b in bQuery on ub.NurseryBranchId equals b.Id
                    where userIds.Contains(ub.UserId) && b.NurseryId == nurseryId
                    select new { ub.UserId, ub.NurseryBranchId });

                byUser = pairs
                    .GroupBy(x => x.UserId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.NurseryBranchId).ToList());
            }

            var result = new List<NurseryUserDto>(users.Count);
            foreach (var user in users)
            {
                var roles = await _identityUserManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault(r => AllowedRoles.Contains(r)) ?? string.Empty;
                byUser.TryGetValue(user.Id, out var branchIds);

                result.Add(new NurseryUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    CreationTime = user.CreationTime,
                    BranchIds = branchIds ?? new List<Guid>(),
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

            await ReplaceUserBranchesAsync(user.Id, nurseryId, nursery.TenantId.Value, input.Role, input.BranchIds);

            return await MapToNurseryUserDtoAsync(user, nurseryId);
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

            await ReplaceUserBranchesAsync(user.Id, nurseryId, nursery.TenantId.Value, input.Role, input.BranchIds);

            return await MapToNurseryUserDtoAsync(user, nurseryId);
        }
    }

    private async Task<NurseryUserDto> MapToNurseryUserDtoAsync(IdentityUser user, Guid nurseryId)
    {
        var roles = await _identityUserManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault(r => AllowedRoles.Contains(r)) ?? string.Empty;
        var branchIds = await GetUserBranchIdsForNurseryAsync(user.Id, nurseryId);

        return new NurseryUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = role,
            CreationTime = user.CreationTime,
            BranchIds = branchIds,
        };
    }

    private async Task<List<Guid>> GetUserBranchIdsForNurseryAsync(Guid userId, Guid nurseryId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var ubQuery = await _userBranchRepository.GetQueryableAsync();
            var bQuery = await _nurseryBranchRepository.GetQueryableAsync();

            return await AsyncExecuter.ToListAsync(
                from ub in ubQuery
                join b in bQuery on ub.NurseryBranchId equals b.Id
                where ub.UserId == userId && b.NurseryId == nurseryId
                select ub.NurseryBranchId);
        }
    }

    private async Task ReplaceUserBranchesAsync(
        Guid userId,
        Guid nurseryId,
        Guid nurseryTenantId,
        string role,
        IReadOnlyCollection<Guid>? branchIds)
    {
        var existing = await _userBranchRepository.GetListAsync(x => x.UserId == userId);
        if (existing.Count > 0)
        {
            await _userBranchRepository.DeleteManyAsync(existing, autoSave: true);
        }

        IReadOnlyList<Guid> targetBranchIds;
        if (role == NurseryHubRoles.NurseryAdmin)
        {
            targetBranchIds = await GetAllBranchIdsForNurseryAsync(nurseryId);
        }
        else if (role == NurseryHubRoles.Teacher || role == NurseryHubRoles.BranchManager)
        {
            if (branchIds == null || branchIds.Count == 0)
            {
                throw new UserFriendlyException(L["NurseryUsers:Validation:BranchesRequired"]);
            }

            var distinct = branchIds.Distinct().ToList();
            await ValidateBranchIdsForNurseryAsync(nurseryId, distinct);
            targetBranchIds = distinct;
        }
        else
        {
            targetBranchIds = Array.Empty<Guid>();
        }

        if (targetBranchIds.Count == 0)
        {
            return;
        }

        var entities = targetBranchIds
            .Select(branchId => new UserBranch(GuidGenerator.Create(), nurseryTenantId, userId, branchId))
            .ToList();
        await _userBranchRepository.InsertManyAsync(entities, autoSave: true);
    }

    private async Task<List<Guid>> GetAllBranchIdsForNurseryAsync(Guid nurseryId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _nurseryBranchRepository.GetQueryableAsync();
            return await AsyncExecuter.ToListAsync(query.Where(b => b.NurseryId == nurseryId).Select(b => b.Id));
        }
    }

    private async Task ValidateBranchIdsForNurseryAsync(Guid nurseryId, List<Guid> branchIds)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _nurseryBranchRepository.GetQueryableAsync();
            var validCount = await AsyncExecuter.CountAsync(
                query.Where(b => b.NurseryId == nurseryId && branchIds.Contains(b.Id)));
            if (validCount != branchIds.Count)
            {
                throw new UserFriendlyException("One or more branches are invalid for this nursery.");
            }
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
