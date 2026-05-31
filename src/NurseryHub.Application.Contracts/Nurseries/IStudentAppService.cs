using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IStudentAppService
    : ICrudAppService<StudentDto, Guid, GetStudentsInput, CreateUpdateStudentDto, CreateUpdateStudentDto>
{
    Task<StudentDto> UploadImageAsync(Guid id, UploadStudentImageInput input);

    Task<StudentDto> AssignClassAsync(Guid id, AssignStudentClassDto input);
}