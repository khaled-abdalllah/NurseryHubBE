using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Locations;

public interface IGovernorateAppService
    : ICrudAppService<GovernorateDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateGovernorateDto, CreateUpdateGovernorateDto>
{
}
