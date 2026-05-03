using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Locations;
using NurseryHub.Security;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Locations;

[Authorize(Roles = NurseryHubRoles.Admin)]
public class GovernorateAppService
    : CrudAppService<
            Governorate,
            GovernorateDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateGovernorateDto,
            CreateUpdateGovernorateDto>,
        IGovernorateAppService
{
    public GovernorateAppService(IRepository<Governorate, Guid> repository)
        : base(repository)
    {
    }

    protected override async Task<Governorate> MapToEntityAsync(CreateUpdateGovernorateDto createInput)
    {
        return await Task.FromResult(
            new Governorate(GuidGenerator.Create(), createInput.Code, createInput.NameEn, createInput.NameAr));
    }

    protected override async Task MapToEntityAsync(CreateUpdateGovernorateDto updateInput, Governorate entity)
    {
        entity.SetCode(updateInput.Code);
        entity.SetNameEn(updateInput.NameEn);
        entity.SetNameAr(updateInput.NameAr);
        await Task.CompletedTask;
    }

    protected override GovernorateDto MapToGetOutputDto(Governorate entity)
    {
        return new GovernorateDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
        };
    }

    protected override IQueryable<Governorate> ApplyDefaultSorting(IQueryable<Governorate> query)
    {
        return query.OrderBy(g => g.NameEn);
    }
}
