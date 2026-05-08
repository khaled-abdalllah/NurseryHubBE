using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class DailyFollowupBook : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxTeacherNoteLength = 2000;
    public const int MaxSleepDurationLength = 64;

    public Guid? TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid NurseryBranchId { get; private set; }
    public DateOnly ReportDate { get; private set; }
    public DailyFollowupMood OverallMood { get; private set; }
    public string TeacherNote { get; private set; } = string.Empty;
    public bool SleptToday { get; private set; }
    public string? SleepDuration { get; private set; }
    public DailyFollowupNapMood? MoodAfterWaking { get; private set; }
    public bool IsDraft { get; private set; }
    public bool SentToParent { get; private set; }
    public bool IsVisibleToParent { get; private set; }
    public DateTime? SentToParentAt { get; private set; }

    public ICollection<DailyFollowupSubjectEntry> Subjects { get; private set; } = new List<DailyFollowupSubjectEntry>();
    public ICollection<DailyFollowupActivityEntry> Activities { get; private set; } = new List<DailyFollowupActivityEntry>();
    public ICollection<DailyFollowupMealEntry> Meals { get; private set; } = new List<DailyFollowupMealEntry>();

    protected DailyFollowupBook()
    {
    }

    public DailyFollowupBook(
        Guid id,
        Guid? tenantId,
        Guid studentId,
        Guid nurseryBranchId,
        DateOnly reportDate,
        DailyFollowupMood overallMood,
        string teacherNote,
        bool sleptToday,
        string? sleepDuration,
        DailyFollowupNapMood? moodAfterWaking) : base(id)
    {
        TenantId = tenantId;
        StudentId = studentId;
        NurseryBranchId = nurseryBranchId;
        ReportDate = reportDate;
        SetOverallMood(overallMood);
        SetTeacherNote(teacherNote);
        SetSleep(sleptToday, sleepDuration, moodAfterWaking);
        SetDraft();
    }

    public void SetOverallMood(DailyFollowupMood mood)
    {
        OverallMood = mood;
    }

    public void SetTeacherNote(string teacherNote)
    {
        TeacherNote = Check.NotNull(teacherNote, nameof(teacherNote), MaxTeacherNoteLength);
    }

    public void SetSleep(bool sleptToday, string? sleepDuration, DailyFollowupNapMood? moodAfterWaking)
    {
        SleptToday = sleptToday;
        if (!sleptToday)
        {
            SleepDuration = null;
            MoodAfterWaking = null;
            return;
        }

        SleepDuration = Check.NotNullOrWhiteSpace(sleepDuration, nameof(sleepDuration), MaxSleepDurationLength);
        MoodAfterWaking = moodAfterWaking ?? throw new ArgumentException("Mood after waking is required when child slept.");
    }

    public void SetDraft()
    {
        IsDraft = true;
        IsVisibleToParent = false;
    }

    public void MarkSentToParent(DateTime sentAt)
    {
        IsDraft = false;
        SentToParent = true;
        IsVisibleToParent = true;
        SentToParentAt = sentAt;
    }
}
