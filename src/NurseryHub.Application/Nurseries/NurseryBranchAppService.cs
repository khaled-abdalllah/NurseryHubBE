using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Locations;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

[Authorize(Roles = NurseryHubRoles.Admin)]
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
    private readonly IRepository<Nursery, Guid> _nurseryRepository;
    private readonly IRepository<Governorate, Guid> _governorateRepository;
    private readonly IRepository<City, Guid> _cityRepository;
    private readonly IDataFilter _dataFilter;

    public NurseryBranchAppService(
        IRepository<NurseryBranch, Guid> repository,
        IRepository<Nursery, Guid> nurseryRepository,
        IRepository<Governorate, Guid> governorateRepository,
        IRepository<City, Guid> cityRepository,
        IDataFilter dataFilter)
        : base(repository)
    {
        _nurseryRepository = nurseryRepository;
        _governorateRepository = governorateRepository;
        _cityRepository = cityRepository;
        _dataFilter = dataFilter;
    }

    public override async Task<PagedResultDto<NurseryBranchDto>> GetListAsync(GetNurseryBranchesInput input)
    {
        // Host admin: nurseries use each nursery's TenantId, so IMultiTenant hides them from the join unless disabled.
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var branches = await Repository.GetQueryableAsync();
            var nurseries = await _nurseryRepository.GetQueryableAsync();
            var governorates = await _governorateRepository.GetQueryableAsync();
            var cities = await _cityRepository.GetQueryableAsync();

            var filtered = branches.WhereIf(input.NurseryId.HasValue, x => x.NurseryId == input.NurseryId);

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

    public override async Task<NurseryBranchDto> GetAsync(Guid id)
    {
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
