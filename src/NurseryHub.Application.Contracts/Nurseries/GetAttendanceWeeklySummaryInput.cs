using System;

namespace NurseryHub.Nurseries;

public class GetAttendanceWeeklySummaryInput
{
    public Guid NurseryBranchId { get; set; }

    /// <summary>Last day of the range (inclusive). Defaults to today (UTC date).</summary>
    public DateOnly? EndDate { get; set; }

    public int DayCount { get; set; } = 7;
}
