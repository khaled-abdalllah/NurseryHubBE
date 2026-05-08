using System;

namespace NurseryHub.Nurseries;

public class AttendanceDayRowDto
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public decimal AgeYearsApprox { get; set; }
    public string? ProfileImageUrl { get; set; }
    public AttendanceStatus? Status { get; set; }
    public Guid? AttendanceId { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}
