using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryBranchAppService
    : ICrudAppService<NurseryBranchDto, Guid, GetNurseryBranchesInput, CreateUpdateNurseryBranchDto, CreateUpdateNurseryBranchDto>
{
}
