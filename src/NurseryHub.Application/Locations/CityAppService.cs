using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Locations;
using NurseryHub.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Locations;

[Authorize(NurseryHubPermissions.Cities.Default)]
public class CityAppService
    : CrudAppService<
            City,
            CityDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateCityDto,
            CreateUpdateCityDto>,
        ICityAppService
{
    private readonly IRepository<Governorate, Guid> _governorateRepository;

    public CityAppService(
        IRepository<City, Guid> repository,
        IRepository<Governorate, Guid> governorateRepository)
        : base(repository)
    {
        _governorateRepository = governorateRepository;
        GetPolicyName = NurseryHubPermissions.Cities.Default;
        GetListPolicyName = NurseryHubPermissions.Cities.Default;
        CreatePolicyName = NurseryHubPermissions.Cities.Create;
        UpdatePolicyName = NurseryHubPermissions.Cities.Edit;
        DeletePolicyName = NurseryHubPermissions.Cities.Delete;
    }

    protected override async Task<City> MapToEntityAsync(CreateUpdateCityDto createInput)
    {
        return await Task.FromResult(
            new City(
                GuidGenerator.Create(),
                createInput.GovernorateId,
                createInput.Code,
                createInput.NameEn,
                createInput.NameAr));
    }

    protected override async Task MapToEntityAsync(CreateUpdateCityDto updateInput, City entity)
    {
        entity.SetGovernorateId(updateInput.GovernorateId);
        entity.SetCode(updateInput.Code);
        entity.SetNameEn(updateInput.NameEn);
        entity.SetNameAr(updateInput.NameAr);
        await Task.CompletedTask;
    }

    public override async Task<PagedResultDto<CityDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var cityQuery = await Repository.GetQueryableAsync();
        var govQuery = await _governorateRepository.GetQueryableAsync();

        var query =
            from city in cityQuery
            join gov in govQuery on city.GovernorateId equals gov.Id
            select new CityDto
            {
                Id = city.Id,
                GovernorateId = city.GovernorateId,
                GovernorateNameEn = gov.NameEn,
                Code = city.Code,
                NameEn = city.NameEn,
                NameAr = city.NameAr,
            };

        var totalCount = await AsyncExecuter.CountAsync(query);

        var ordered = query.OrderBy(c => c.NameEn);
        var items = await AsyncExecuter.ToListAsync(
            ordered.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<CityDto>(totalCount, items);
    }

    public override async Task<CityDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var entity = await Repository.GetAsync(id);
        var gov = await _governorateRepository.GetAsync(entity.GovernorateId);
        return new CityDto
        {
            Id = entity.Id,
            GovernorateId = entity.GovernorateId,
            GovernorateNameEn = gov.NameEn,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
        };
    }
}
