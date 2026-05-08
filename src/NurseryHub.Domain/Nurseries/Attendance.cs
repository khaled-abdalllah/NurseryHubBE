using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

/// <summary>
/// One attendance record per student per calendar day (unique on StudentId + Date).
/// Tenant scope follows <see cref="IMultiTenant"/>; branch is enforced via <see cref="Student"/>.
/// </summary>
public class Attendance : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid StudentId { get; private set; }
    public DateOnly Date { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public DateTime? CheckedInAt { get; private set; }
    public DateTime? CheckedOutAt { get; private set; }

    protected Attendance()
    {
    }

    public Attendance(Guid id, Guid? tenantId, Guid studentId, DateOnly date, AttendanceStatus status)
        : base(id)
    {
        TenantId = tenantId;
        StudentId = studentId;
        Date = date;
        Status = status;
    }

    public void SetStatus(AttendanceStatus status)
    {
        Status = status;
    }

    public void SetCheckTimes(DateTime? checkedInAt, DateTime? checkedOutAt)
    {
        CheckedInAt = checkedInAt;
        CheckedOutAt = checkedOutAt;
    }
}
