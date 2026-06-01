using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IAttendanceAppService : IApplicationService
{
    Task<AttendanceDayRowDto> MarkAsync(MarkAttendanceInput input);

    Task<IReadOnlyList<AttendanceDayRowDto>> BulkMarkAsync(BulkMarkAttendanceInput input);

    Task<IReadOnlyList<AttendanceDayRowDto>> GetByDateAsync(GetAttendanceByDateInput input);

    Task<IReadOnlyList<AttendanceWeeklyDaySummaryDto>> GetWeeklySummaryAsync(GetAttendanceWeeklySummaryInput input);

    Task<IReadOnlyList<AttendanceHistoryItemDto>> GetHistoryAsync(GetAttendanceHistoryInput input);
}
