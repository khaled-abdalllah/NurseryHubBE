using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace NurseryHub.Nurseries;

public interface INurseryAppService
    : ICrudAppService<NurseryDto, Guid, GetNurseriesInput, CreateUpdateNurseryDto, CreateUpdateNurseryDto>
{
    Task<NurseryDto> UploadLogoAsync(Guid id, UploadNurseryLogoInput input);

    Task<IRemoteStreamContent> GetLogoAsync(Guid id);
}
