using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Locations;

public interface ICityAppService
    : ICrudAppService<CityDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCityDto, CreateUpdateCityDto>
{
}
