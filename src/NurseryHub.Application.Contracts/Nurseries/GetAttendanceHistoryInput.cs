using System;

namespace NurseryHub.Nurseries;

public class GetAttendanceHistoryInput
{
    public Guid NurseryBranchId { get; set; }
    public Guid StudentId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
