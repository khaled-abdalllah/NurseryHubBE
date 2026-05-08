using System;

namespace NurseryHub.Nurseries;

public class AttendanceHistoryItemDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}
