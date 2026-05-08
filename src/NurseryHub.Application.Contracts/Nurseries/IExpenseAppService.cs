using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IExpenseAppService : IApplicationService
{
    Task<PagedResultDto<ExpenseListDto>> GetListAsync(ExpenseFilterDto input);
    Task<ExpenseDetailsDto> GetAsync(Guid id);
    Task<ExpenseDto> CreateAsync(CreateUpdateExpenseDto input);
    Task<ExpenseDto> UpdateAsync(Guid id, CreateUpdateExpenseDto input);
    Task DeleteAsync(Guid id);
}
