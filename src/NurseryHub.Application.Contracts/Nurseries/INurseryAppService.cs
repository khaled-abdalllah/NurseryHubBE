using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryAppService
    : ICrudAppService<NurseryDto, Guid, GetNurseriesInput, CreateUpdateNurseryDto, CreateUpdateNurseryDto>
{
    Task<NurseryDto> UploadLogoAsync(Guid id, UploadNurseryLogoInput input);
}
