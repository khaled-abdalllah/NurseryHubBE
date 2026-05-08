using System;

namespace NurseryHub.Nurseries;

public class MarkAttendanceInput
{
    public Guid NurseryBranchId { get; set; }
    public Guid StudentId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}
