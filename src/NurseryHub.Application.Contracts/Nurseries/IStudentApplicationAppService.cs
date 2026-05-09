using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IStudentApplicationAppService : ICrudAppService<
    StudentApplicationDto,
    Guid,
    StudentApplicationPagedRequestDto,
    CreateUpdateStudentApplicationDto,
    CreateUpdateStudentApplicationDto>
{
    Task<StudentApplicationSummaryDto> GetSummaryAsync(Guid nurseryBranchId);

    Task<StudentApplicationDto> ChangeStatusAsync(Guid id, ChangeStudentApplicationStatusDto input);

    Task<StudentDto> ConvertToStudentAsync(Guid id, ConvertStudentApplicationDto input);
}
