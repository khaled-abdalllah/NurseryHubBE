using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryUserAppService : IApplicationService
{
    Task<List<NurseryUserDto>> GetListAsync(Guid nurseryId);

    Task<NurseryUserDto> CreateAsync(Guid nurseryId, CreateNurseryUserDto input);

    Task<NurseryUserDto> UpdateAsync(Guid nurseryId, Guid userId, UpdateNurseryUserDto input);
}
