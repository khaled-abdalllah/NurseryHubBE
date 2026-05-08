using System;
using System.Collections.Generic;

namespace NurseryHub.Nurseries;

public class BulkMarkAttendanceInput
{
    public Guid NurseryBranchId { get; set; }
    public DateOnly Date { get; set; }
    public IReadOnlyList<BulkMarkAttendanceItemDto> Items { get; set; } = Array.Empty<BulkMarkAttendanceItemDto>();
}

public class BulkMarkAttendanceItemDto
{
    public Guid StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}
