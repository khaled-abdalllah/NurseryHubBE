using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class DailyFollowupSubjectEntry : Entity<Guid>, IMultiTenant
{
    public const int MaxSubjectNameLength = 128;
    public const int MaxSubjectIconLength = 64;
    public const int MaxNotesLength = 1000;

    public Guid? TenantId { get; private set; }
    public Guid DailyFollowupBookId { get; private set; }
    public string SubjectName { get; private set; } = string.Empty;
    public string SubjectIcon { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public int SortOrder { get; private set; }

    protected DailyFollowupSubjectEntry()
    {
    }

    public DailyFollowupSubjectEntry(
        Guid id,
        Guid? tenantId,
        Guid dailyFollowupBookId,
        string subjectName,
        string subjectIcon,
        bool isActive,
        string? notes,
        int sortOrder) : base(id)
    {
        TenantId = tenantId;
        DailyFollowupBookId = dailyFollowupBookId;
        SubjectName = Check.NotNullOrWhiteSpace(subjectName, nameof(subjectName), MaxSubjectNameLength);
        SubjectIcon = Check.NotNullOrWhiteSpace(subjectIcon, nameof(subjectIcon), MaxSubjectIconLength);
        IsActive = isActive;
        Notes = Check.Length(notes, nameof(notes), MaxNotesLength);
        SortOrder = sortOrder;
    }
}
