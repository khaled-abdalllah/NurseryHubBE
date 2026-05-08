using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryBranchAppService
    : ICrudAppService<NurseryBranchDto, Guid, GetNurseryBranchesInput, CreateUpdateNurseryBranchDto, CreateUpdateNurseryBranchDto>
{
    Task<ListResultDto<ManagedBranchLookupDto>> GetManagedBranchesAsync();
}
