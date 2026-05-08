using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryClassAppService
    : ICrudAppService<
        NurseryClassDto,
        Guid,
        GetNurseryClassesInput,
        CreateUpdateNurseryClassDto,
        CreateUpdateNurseryClassDto>
{
}
