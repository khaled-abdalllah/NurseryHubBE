using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IPaymentAppService : IApplicationService
{
    Task<PagedResultDto<PaymentListDto>> GetListAsync(PaymentFilterDto input);
    Task<PaymentDetailsDto> GetAsync(Guid id);
    Task<PaymentDto> CreateAsync(CreateUpdatePaymentDto input);
    Task<PaymentDto> UpdateAsync(Guid id, CreateUpdatePaymentDto input);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<PaymentStudentLookupDto>> GetStudentsByClassAsync(GetStudentsByClassInput input);
}
