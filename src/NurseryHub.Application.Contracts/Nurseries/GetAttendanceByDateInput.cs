using System;

namespace NurseryHub.Nurseries;

public class GetAttendanceByDateInput
{
    public Guid NurseryBranchId { get; set; }
    public DateOnly Date { get; set; }
    public Guid? NurseryClassId { get; set; }
}
