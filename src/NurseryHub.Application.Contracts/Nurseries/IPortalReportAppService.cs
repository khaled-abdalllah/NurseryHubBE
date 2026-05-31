using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IPortalReportAppService : IApplicationService
{
    Task<PortalReportResultDto> GenerateAsync(PortalReportInput input);
}
