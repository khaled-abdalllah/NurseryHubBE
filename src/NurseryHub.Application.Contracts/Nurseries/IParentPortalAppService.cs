using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IParentPortalAppService : IApplicationService
{
    Task<List<ParentPortalStudentDto>> GetStudentsAsync();

    Task<PagedResultDto<ParentFollowupTimelineItemDto>> GetFollowUpTimelineAsync(GetParentFollowupTimelineInput input);

    Task<ParentFollowupDetailsDto> GetFollowUpDetailsAsync(Guid id);

    Task<ParentStudentAttendanceDayDto> GetAttendanceAsync(GetParentStudentAttendanceInput input);
}
