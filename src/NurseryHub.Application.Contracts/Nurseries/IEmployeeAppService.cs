using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IEmployeeAppService : IApplicationService
{
    Task<PagedResultDto<EmployeeListDto>> GetListAsync(EmployeeFilterDto input);
    Task<EmployeeDetailsDto> GetAsync(Guid id);
    Task<EmployeeDetailsDto> CreateAsync(CreateUpdateEmployeeDto input);
    Task<EmployeeDetailsDto> UpdateAsync(Guid id, CreateUpdateEmployeeDto input);
    Task DeleteAsync(Guid id);
    Task ChangeStatusAsync(Guid id, ChangeEmployeeStatusDto input);
    Task<EmployeeSummaryDto> GetSummaryAsync();
}
