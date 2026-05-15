using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Locations;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Users;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

[Authorize(Roles =
    $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.BranchManager},{NurseryHubRoles.Teacher},{NurseryHubRoles.Accountant}")]
public class NurseryBranchAppService
    : CrudAppService<
            NurseryBranch,
            NurseryBranchDto,
            Guid,
            GetNurseryBranchesInput,
            CreateUpdateNurseryBranchDto,
            CreateUpdateNurseryBranchDto>,
        INurseryBranchAppService
{
    private readonly NurseryMediaOptions _mediaOptions;
    private readonly IRepository<Nursery, Guid> _nurseryRepository;
    private readonly IRepository<Governorate, Guid> _governorateRepository;
    private readonly IRepository<City, Guid> _cityRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IDataFilter _dataFilter;

    public NurseryBranchAppService(
        IRepository<NurseryBranch, Guid> repository,
        IRepository<Nursery, Guid> nurseryRepository,
        IRepository<Governorate, Guid> governorateRepository,
        IRepository<City, Guid> cityRepository,
        IRepository<UserBranch, Guid> userBranchRepository,
        IdentityUserManager identityUserManager,
        IDataFilter dataFilter,
        IOptions<NurseryMediaOptions> mediaOptions)
        : base(repository)
    {
        _mediaOptions = mediaOptions.Value;
        _nurseryRepository = nurseryRepository;
        _governorateRepository = governorateRepository;
        _cityRepository = cityRepository;
        _userBranchRepository = userBranchRepository;
        _identityUserManager = identityUserManager;
        _dataFilter = dataFilter;
    }

    private static bool UserIsInAnyRole(ICurrentUser user, params string[] roles)
    {
        var userRoles = user.Roles;
        if (userRoles == null || !userRoles.Any())
        {
            return false;
        }

        return roles.Any(
            r => userRoles.Any(ur => string.Equals(ur, r, StringComparison.OrdinalIgnoreCase)));
    }

    private void EnsureCanManageBranchCrud()
    {
        if (!UserIsInAnyRole(CurrentUser, NurseryHubRoles.Admin, NurseryHubRoles.NurseryAdmin))
        {
            throw new AbpAuthorizationException();
        }
    }

    public override async Task<NurseryBranchDto> CreateAsync(CreateUpdateNurseryBranchDto input)
    {
        EnsureCanManageBranchCrud();
        Nursery nursery;
       nursery = await _nurseryRepository.GetAsync(input.NurseryId);


        if (!nursery.TenantId.HasValue)
        {
            throw new UserFriendlyException("Nursery tenant is not configured.");
        }

        NurseryBranchDto dto;
        using (CurrentTenant.Change(nursery.TenantId.Value))
        {
            dto = await base.CreateAsync(input);
        }

        await GrantNurseryAdminsAccessToNewBranchAsync(dto.Id, input.NurseryId);
        return dto;
    }

    private async Task GrantNurseryAdminsAccessToNewBranchAsync(Guid newBranchId, Guid nurseryId)
    {
        Nursery nursery;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            nursery = await _nurseryRepository.GetAsync(nurseryId);
        }

        if (!nursery.TenantId.HasValue)
        {
            return;
        }

        using (CurrentTenant.Change(nursery.TenantId.Value))
        {
            var admins = await _identityUserManager.GetUsersInRoleAsync(NurseryHubRoles.NurseryAdmin);
            if (admins == null || admins.Count == 0)
            {
                return;
            }

            var adminIds = admins.Select(u => u.Id).ToHashSet();
            var existing = await _userBranchRepository.GetListAsync(
                ub => ub.NurseryBranchId == newBranchId && adminIds.Contains(ub.UserId));
            var alreadyLinked = existing.Select(e => e.UserId).ToHashSet();

            var toInsert = new List<UserBranch>();
            foreach (var user in admins)
            {
                if (alreadyLinked.Contains(user.Id))
                {
                    continue;
                }

                toInsert.Add(new UserBranch(GuidGenerator.Create(), nursery.TenantId.Value, user.Id, newBranchId));
            }

            if (toInsert.Count > 0)
            {
                await _userBranchRepository.InsertManyAsync(toInsert, autoSave: true);
            }
        }
    }

    public override async Task<PagedResultDto<NurseryBranchDto>> GetListAsync(GetNurseryBranchesInput input)
    {
        EnsureCanManageBranchCrud();
        // Host admin: nurseries use each nursery's TenantId, so IMultiTenant hides them from the join unless disabled.
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var branches = await Repository.GetQueryableAsync();
            var nurseries = await _nurseryRepository.GetQueryableAsync();
            var governorates = await _governorateRepository.GetQueryableAsync();
            var cities = await _cityRepository.GetQueryableAsync();

            var filtered = branches.WhereIf(input.NurseryId.HasValue, x => x.NurseryId == input.NurseryId);
            // Tenant users must only see branches for their tenant; multitenant is disabled above for nursery joins.
            filtered = filtered.WhereIf(CurrentTenant.Id.HasValue, x => x.TenantId == CurrentTenant.Id);

            var query =
                from branch in filtered
                join nursery in nurseries on branch.NurseryId equals nursery.Id
                join governorate in governorates on branch.GovernorateId equals governorate.Id
                join city in cities on branch.CityId equals city.Id
                select new NurseryBranchDto
                {
                    Id = branch.Id,
                    NurseryId = branch.NurseryId,
                    NurseryName = nursery.Name,
                    Name = branch.Name,
                    PhoneNumber = branch.PhoneNumber,
                    AddressLine = branch.AddressLine,
                    IsTransportationIncluded = branch.IsTransportationIncluded,
                    IsMealsIncluded = branch.IsMealsIncluded,
                    GovernorateId = branch.GovernorateId,
                    GovernorateNameEn = governorate.NameEn,
                    CityId = branch.CityId,
                    CityNameEn = city.NameEn,
                    FacebookLink = branch.FacebookLink,
                    IsActive = branch.IsActive,
                    CreationTime = branch.CreationTime,
                    CreatorId = branch.CreatorId,
                    LastModificationTime = branch.LastModificationTime,
                    LastModifierId = branch.LastModifierId,
                    IsDeleted = branch.IsDeleted,
                    DeleterId = branch.DeleterId,
                    DeletionTime = branch.DeletionTime,
                };

            var totalCount = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(
                query.OrderBy(x => x.Name).Skip(input.SkipCount).Take(input.MaxResultCount));

            return new PagedResultDto<NurseryBranchDto>(totalCount, items);
        }
    }

    public async Task<ListResultDto<ManagedBranchLookupDto>> GetManagedBranchesAsync()
    {
        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            return new ListResultDto<ManagedBranchLookupDto>(new List<ManagedBranchLookupDto>());
        }


            var branches = await Repository.GetQueryableAsync();
            var nurseries = await _nurseryRepository.GetQueryableAsync();
            var userBranches = await _userBranchRepository.GetQueryableAsync();

            var query =
                from ub in userBranches
                join branch in branches on ub.NurseryBranchId equals branch.Id
                join nursery in nurseries on branch.NurseryId equals nursery.Id
                where ub.UserId == CurrentUser.Id.Value
                      && ub.TenantId == CurrentTenant.Id
                select new
                {
                    Id = branch.Id,
                    Name = branch.Name,
                    NurseryName = nursery.Name,
                    NurseryLogoUrlStored = nursery.LogoUrl,
                };

            var rows = await AsyncExecuter.ToListAsync(
                query.Distinct().OrderBy(x => x.Name));
            var items = rows
                .Select(x => new ManagedBranchLookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    NurseryName = x.NurseryName,
                    NurseryLogoUrl = ResolveLogoDisplayUrl(x.NurseryLogoUrlStored),
                })
                .ToList();

            return new ListResultDto<ManagedBranchLookupDto>(items);
    }

    private string? ResolveLogoDisplayUrl(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return null;
        }

        if (stored.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) ||
            stored.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
        {
            return stored;
        }

        var baseUrl = _mediaOptions.PublicBaseUrl.TrimEnd('/');
        var fileName = stored.Replace('\\', '/').TrimStart('/');
        if (fileName.Contains('/', System.StringComparison.Ordinal))
        {
            return $"{baseUrl}/{fileName}";
        }

        return $"{baseUrl}/logo/{fileName}";
    }

    public override async Task<NurseryBranchDto> UpdateAsync(Guid id, CreateUpdateNurseryBranchDto input)
    {
        EnsureCanManageBranchCrud();
        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        EnsureCanManageBranchCrud();
        await base.DeleteAsync(id);
    }

    public override async Task<NurseryBranchDto> GetAsync(Guid id)
    {
        EnsureCanManageBranchCrud();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var entity = await Repository.GetAsync(id);
            var nursery = await _nurseryRepository.GetAsync(entity.NurseryId);
            var governorate = await _governorateRepository.GetAsync(entity.GovernorateId);
            var city = await _cityRepository.GetAsync(entity.CityId);

            return new NurseryBranchDto
            {
                Id = entity.Id,
                NurseryId = entity.NurseryId,
                NurseryName = nursery.Name,
                Name = entity.Name,
                PhoneNumber = entity.PhoneNumber,
                AddressLine = entity.AddressLine,
                IsTransportationIncluded = entity.IsTransportationIncluded,
                IsMealsIncluded = entity.IsMealsIncluded,
                GovernorateId = entity.GovernorateId,
                GovernorateNameEn = governorate.NameEn,
                CityId = entity.CityId,
                CityNameEn = city.NameEn,
                FacebookLink = entity.FacebookLink,
                IsActive = entity.IsActive,
                CreationTime = entity.CreationTime,
                CreatorId = entity.CreatorId,
                LastModificationTime = entity.LastModificationTime,
                LastModifierId = entity.LastModifierId,
                IsDeleted = entity.IsDeleted,
                DeleterId = entity.DeleterId,
                DeletionTime = entity.DeletionTime,
            };
        }
    }

    protected override async Task<NurseryBranch> GetEntityByIdAsync(Guid id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await Repository.GetAsync(id);
        }
    }

    protected override async Task<NurseryBranch> MapToEntityAsync(CreateUpdateNurseryBranchDto createInput)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var nursery = await _nurseryRepository.GetAsync(createInput.NurseryId);
            return new NurseryBranch(
                GuidGenerator.Create(),
                nursery.TenantId,
                createInput.NurseryId,
                createInput.Name,
                createInput.PhoneNumber,
                createInput.GovernorateId,
                createInput.CityId,
                createInput.IsTransportationIncluded,
                createInput.IsMealsIncluded,
                createInput.AddressLine,
                createInput.FacebookLink,
                createInput.IsActive);
        }
    }

    protected override async Task MapToEntityAsync(CreateUpdateNurseryBranchDto updateInput, NurseryBranch entity)
    {
        entity.SetName(updateInput.Name);
        entity.SetPhoneNumber(updateInput.PhoneNumber);
        entity.SetAddressLine(updateInput.AddressLine);
        entity.SetTransportationIncluded(updateInput.IsTransportationIncluded);
        entity.SetMealsIncluded(updateInput.IsMealsIncluded);
        entity.SetLocation(updateInput.GovernorateId, updateInput.CityId);
        entity.SetFacebookLink(updateInput.FacebookLink);
        entity.SetIsActive(updateInput.IsActive);
        await Task.CompletedTask;
    }

    protected override NurseryBranchDto MapToGetOutputDto(NurseryBranch entity)
    {
        // Used by CrudAppService create/update response mapping.
        // Enriched names are provided in GetAsync/GetListAsync overrides.
        return new NurseryBranchDto
        {
            Id = entity.Id,
            NurseryId = entity.NurseryId,
            Name = entity.Name,
            PhoneNumber = entity.PhoneNumber,
            AddressLine = entity.AddressLine,
            IsTransportationIncluded = entity.IsTransportationIncluded,
            IsMealsIncluded = entity.IsMealsIncluded,
            GovernorateId = entity.GovernorateId,
            CityId = entity.CityId,
            FacebookLink = entity.FacebookLink,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime,
            NurseryName = null,
            GovernorateNameEn = null,
            CityNameEn = null,
        };
    }
}
