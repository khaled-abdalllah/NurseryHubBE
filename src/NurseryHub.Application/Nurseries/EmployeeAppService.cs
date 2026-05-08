using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace NurseryHub.Nurseries;

[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
public class EmployeeAppService : ApplicationService, IEmployeeAppService
{
    private const string DefaultEmployeePassword = "Aa@12345678";

    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;
    private readonly IRepository<NurseryBranch, Guid> _branchRepository;
    private readonly IIdentityRoleRepository _identityRoleRepository;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IdentityUserManager _identityUserManager;

    public EmployeeAppService(
        IIdentityUserRepository identityUserRepository,
        IRepository<UserBranch, Guid> userBranchRepository,
        IRepository<NurseryBranch, Guid> branchRepository,
        IIdentityRoleRepository identityRoleRepository,
        IdentityRoleManager identityRoleManager,
        IdentityUserManager identityUserManager)
    {
        _identityUserRepository = identityUserRepository;
        _userBranchRepository = userBranchRepository;
        _branchRepository = branchRepository;
        _identityRoleRepository = identityRoleRepository;
        _identityRoleManager = identityRoleManager;
        _identityUserManager = identityUserManager;
    }

    public async Task<PagedResultDto<EmployeeListDto>> GetListAsync(EmployeeFilterDto input)
    {
        var users = (await _identityUserRepository.GetListAsync())
            .Where(x => x.TenantId == CurrentTenant.Id)
            .ToList();
        var branches = await _branchRepository.GetListAsync(x => x.TenantId == CurrentTenant.Id);
        var userBranches = (await _userBranchRepository.GetListAsync())
            .Where(x => x.TenantId == CurrentTenant.Id)
            .ToList();
        var branchById = branches.ToDictionary(x => x.Id, x => x.Name);
        var branchIdsByUser = userBranches
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.NurseryBranchId).Distinct().ToList());

        var result = new List<EmployeeListDto>(users.Count);
        foreach (var user in users)
        {
            var role = await ResolveRoleAsync(user);
            if (!branchIdsByUser.TryGetValue(user.Id, out var assignedIds) || assignedIds.Count == 0)
            {
                continue;
            }

            var resolvedNames = new List<string>();
            foreach (var bid in assignedIds)
            {
                if (!branchById.TryGetValue(bid, out var bname))
                {
                    resolvedNames.Clear();
                    break;
                }

                resolvedNames.Add(bname);
            }

            if (resolvedNames.Count != assignedIds.Count)
            {
                continue;
            }

            result.Add(new EmployeeListDto
            {
                Id = user.Id,
                FullName = BuildDisplayName(user.Name, user.Surname, user.UserName, user.Email),
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = role,
                BranchIds = assignedIds,
                BranchNames = string.Join(", ", resolvedNames),
                Status = user.IsActive ? EmployeeStatus.Active : EmployeeStatus.Inactive,
                CreationTime = user.CreationTime,
            });
        }

        var filtered = result.AsQueryable()
            .WhereIf(input.Role.HasValue, x => x.Role == input.Role)
            .WhereIf(input.BranchId.HasValue, x => x.BranchIds.Contains(input.BranchId!.Value))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(!input.EmployeeName.IsNullOrWhiteSpace(), x => x.FullName.Contains(input.EmployeeName!))
            .WhereIf(!input.Email.IsNullOrWhiteSpace(), x => x.Email.Contains(input.Email!));

        var totalCount = filtered.Count();
        var items = ApplySorting(filtered, input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<EmployeeListDto>(totalCount, items);
    }

    public async Task<EmployeeDetailsDto> GetAsync(Guid id)
    {
        var user = await _identityUserRepository.GetAsync(id);
        EnsureTenant(user.TenantId);

        var (branchIds, branchNames) = await GetAssignedBranchesAsync(id);
        var role = await ResolveRoleAsync(user);
        return new EmployeeDetailsDto
        {
            Id = user.Id,
            FullName = BuildFullName(user),
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = role,
            BranchIds = branchIds,
            BranchNames = branchNames,
            Status = user.IsActive ? EmployeeStatus.Active : EmployeeStatus.Inactive,
            CreationTime = user.CreationTime,
        };
    }

    public async Task<EmployeeDetailsDto> CreateAsync(CreateUpdateEmployeeDto input)
    {
        await ValidateCreateOrUpdateInputAsync(input, null);
        await EnsureRoleExistsAsync(MapRoleToSystemRole(input.Role));

        var (name, surname) = SplitName(input.FullName);
        var userName = BuildUserNameFromEmail(input.Email);
        var user = new IdentityUser(GuidGenerator.Create(), userName, input.Email, CurrentTenant.Id)
        {
            Name = name,
            Surname = surname,
        };
        user.SetIsActive(input.IsActive);

        var createResult = await _identityUserManager.CreateAsync(user, DefaultEmployeePassword);
        ThrowIfFailed(createResult);
        var setPhoneResult = await _identityUserManager.SetPhoneNumberAsync(user, input.PhoneNumber);
        ThrowIfFailed(setPhoneResult);

        var roleName = MapRoleToSystemRole(input.Role);
        var addRoleResult = await _identityUserManager.AddToRoleAsync(user, roleName);
        ThrowIfFailed(addRoleResult);

        await ReplaceUserBranchesAsync(user.Id, input.BranchIds);
        return await GetAsync(user.Id);
    }

    public async Task<EmployeeDetailsDto> UpdateAsync(Guid id, CreateUpdateEmployeeDto input)
    {
        await ValidateCreateOrUpdateInputAsync(input, id);
        await EnsureRoleExistsAsync(MapRoleToSystemRole(input.Role));

        var user = await _identityUserRepository.GetAsync(id);
        EnsureTenant(user.TenantId);

        var (name, surname) = SplitName(input.FullName);
        user.Name = name;
        user.Surname = surname;
        user.SetIsActive(input.IsActive);
        var setPhoneResult = await _identityUserManager.SetPhoneNumberAsync(user, input.PhoneNumber);
        ThrowIfFailed(setPhoneResult);

        var setEmailResult = await _identityUserManager.SetEmailAsync(user, input.Email);
        ThrowIfFailed(setEmailResult);
        var setUserNameResult = await _identityUserManager.SetUserNameAsync(user, BuildUserNameFromEmail(input.Email));
        ThrowIfFailed(setUserNameResult);

        var existingRoles = await _identityUserManager.GetRolesAsync(user);
        foreach (var role in existingRoles.Where(IsEmployeeRoleName))
        {
            var removeRoleResult = await _identityUserManager.RemoveFromRoleAsync(user, role);
            ThrowIfFailed(removeRoleResult);
        }

        var addRoleResult = await _identityUserManager.AddToRoleAsync(user, MapRoleToSystemRole(input.Role));
        ThrowIfFailed(addRoleResult);

        var updateResult = await _identityUserManager.UpdateAsync(user);
        ThrowIfFailed(updateResult);

        await ReplaceUserBranchesAsync(user.Id, input.BranchIds);
        return await GetAsync(user.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _identityUserRepository.GetAsync(id);
        EnsureTenant(user.TenantId);

        var userBranchLinks = await _userBranchRepository.GetListAsync(x => x.UserId == id);
        if (userBranchLinks.Count > 0)
        {
            await _userBranchRepository.DeleteManyAsync(userBranchLinks, autoSave: true);
        }

        await _identityUserRepository.DeleteAsync(user, autoSave: true);
    }

    public async Task ChangeStatusAsync(Guid id, ChangeEmployeeStatusDto input)
    {
        var user = await _identityUserRepository.GetAsync(id);
        EnsureTenant(user.TenantId);
        user.SetIsActive(input.IsActive);
        var updateResult = await _identityUserManager.UpdateAsync(user);
        ThrowIfFailed(updateResult);
    }

    public async Task<EmployeeSummaryDto> GetSummaryAsync()
    {
        var list = await GetListAsync(new EmployeeFilterDto
        {
            SkipCount = 0,
            MaxResultCount = int.MaxValue,
            Sorting = "creationTime desc",
        });

        return new EmployeeSummaryDto
        {
            TotalEmployees = (int)list.TotalCount,
            ActiveEmployees = list.Items.Count(x => x.Status == EmployeeStatus.Active),
            InactiveEmployees = list.Items.Count(x => x.Status == EmployeeStatus.Inactive),
            TeachersCount = list.Items.Count(x => x.Role == EmployeeRole.Teacher),
            AccountantsCount = list.Items.Count(x => x.Role == EmployeeRole.Accountant),
        };
    }

    private async Task ValidateCreateOrUpdateInputAsync(CreateUpdateEmployeeDto input, Guid? updatingUserId)
    {
        if (!Enum.IsDefined(typeof(EmployeeRole), input.Role))
        {
            throw new UserFriendlyException(L["NurseryHub:Employees:InvalidRole"]);
        }

        var ids = input.BranchIds?.Where(x => x != Guid.Empty).Distinct().ToList()
                  ?? new List<Guid>();
        if (ids.Count == 0)
        {
            throw new UserFriendlyException(L["NurseryHub:Employees:InvalidBranch"]);
        }

        foreach (var branchId in ids)
        {
            var branch = await _branchRepository.FindAsync(branchId);
            if (branch == null || branch.TenantId != CurrentTenant.Id)
            {
                throw new UserFriendlyException(L["NurseryHub:Employees:InvalidBranch"]);
            }
        }

        var normalizedEmail = input.Email.Trim().ToUpperInvariant();
        var existing = await _identityUserRepository.FindByNormalizedEmailAsync(normalizedEmail);
        if (existing != null && existing.Id != updatingUserId)
        {
            throw new UserFriendlyException(L["NurseryHub:Employees:EmailAlreadyExists"]);
        }
    }

    private async Task ReplaceUserBranchesAsync(Guid userId, IReadOnlyList<Guid> branchIds)
    {
        var orderedDistinct = branchIds.Where(x => x != Guid.Empty).Distinct().ToList();

        var existing = await _userBranchRepository.GetListAsync(x => x.UserId == userId);
        if (existing.Count > 0)
        {
            await _userBranchRepository.DeleteManyAsync(existing, autoSave: true);
        }

        for (var i = 0; i < orderedDistinct.Count; i++)
        {
            var link = new UserBranch(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                userId,
                orderedDistinct[i],
                isPrimary: i == 0);
            await _userBranchRepository.InsertAsync(link, autoSave: true);
        }
    }

    private async Task<(List<Guid> Ids, string NamesJoined)> GetAssignedBranchesAsync(Guid userId)
    {
        var userBranchQuery = await _userBranchRepository.GetQueryableAsync();
        var branchQuery = await _branchRepository.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            from ub in userBranchQuery
            join b in branchQuery on ub.NurseryBranchId equals b.Id
            where ub.UserId == userId
            orderby ub.IsPrimary descending, b.Name
            select new { b.Id, b.Name });

        if (rows.Count == 0)
        {
            throw new UserFriendlyException(L["NurseryHub:Employees:BranchAssignmentNotFound"]);
        }

        var ids = rows.Select(x => x.Id).ToList();
        var names = string.Join(", ", rows.Select(x => x.Name));
        return (ids, names);
    }

    private async Task<EmployeeRole> ResolveRoleAsync(IdentityUser user)
    {
        var roles = await _identityUserManager.GetRolesAsync(user);
        if (roles.Contains(NurseryHubRoles.Accountant))
        {
            return EmployeeRole.Accountant;
        }

        return EmployeeRole.Teacher;
    }

    private static IQueryable<EmployeeListDto> ApplySorting(IQueryable<EmployeeListDto> query, string? sorting)
    {
        if (sorting.IsNullOrWhiteSpace() || sorting.Contains("creationTime", StringComparison.OrdinalIgnoreCase))
        {
            return sorting?.Contains("asc", StringComparison.OrdinalIgnoreCase) == true
                ? query.OrderBy(x => x.CreationTime)
                : query.OrderByDescending(x => x.CreationTime);
        }

        if (sorting.Contains("fullName", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.FullName)
                : query.OrderBy(x => x.FullName);
        }

        if (sorting.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Email)
                : query.OrderBy(x => x.Email);
        }

        return query.OrderByDescending(x => x.CreationTime);
    }

    private static bool IsEmployeeRoleName(string roleName)
    {
        return roleName.Equals(NurseryHubRoles.Teacher, StringComparison.OrdinalIgnoreCase) ||
               roleName.Equals(NurseryHubRoles.Accountant, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildUserNameFromEmail(string email)
    {
        var normalized = email.Trim();
        return normalized.Contains('@') ? normalized[..normalized.IndexOf('@')] : normalized;
    }

    private static string BuildFullName(IdentityUser user)
    {
        return BuildDisplayName(user.Name, user.Surname, user.UserName, user.Email);
    }

    private static string BuildDisplayName(string? name, string? surname, string? userName, string? email)
    {
        var full = $"{name ?? ""} {surname ?? ""}".Trim();
        if (!full.IsNullOrWhiteSpace())
        {
            return full;
        }

        if (!userName.IsNullOrWhiteSpace())
        {
            return userName!;
        }

        if (!email.IsNullOrWhiteSpace() && email!.Contains('@'))
        {
            return email[..email.IndexOf('@')];
        }

        return string.Empty;
    }

    private static (string Name, string Surname) SplitName(string fullName)
    {
        var clean = fullName.Trim();
        if (clean.IsNullOrWhiteSpace())
        {
            return (string.Empty, string.Empty);
        }

        var parts = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static string MapRoleToSystemRole(EmployeeRole role)
    {
        return role switch
        {
            EmployeeRole.Teacher => NurseryHubRoles.Teacher,
            EmployeeRole.Accountant => NurseryHubRoles.Accountant,
            _ => NurseryHubRoles.Teacher,
        };
    }

    private void EnsureTenant(Guid? tenantId)
    {
        if (tenantId != CurrentTenant.Id)
        {
            throw new UserFriendlyException(L["NurseryHub:Employees:TenantMismatch"]);
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
