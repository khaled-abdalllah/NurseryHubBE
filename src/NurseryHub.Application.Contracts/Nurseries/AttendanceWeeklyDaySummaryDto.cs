using System;

namespace NurseryHub.Nurseries;

public class AttendanceWeeklyDaySummaryDto
{
    public DateOnly Date { get; set; }

    public int PresentCount { get; set; }

    public int TotalCount { get; set; }
}
