using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INurseryStatisticsAppService : IApplicationService
{
    Task<NurseryStatisticsDto> GetStatisticsAsync();

    Task<NurseryClassBranchStatisticsDto> GetBranchClassStatisticsAsync(Guid nurseryBranchId);
}
