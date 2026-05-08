using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class DailyFollowupBookDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid NurseryBranchId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? ClassroomName { get; set; }
    public string? StudentProfileImageUrl { get; set; }
    public DateOnly ReportDate { get; set; }
    public DailyFollowupMood OverallMood { get; set; }
    public string TeacherNote { get; set; } = string.Empty;
    public bool SleptToday { get; set; }
    public string? SleepDuration { get; set; }
    public DailyFollowupNapMood? MoodAfterWaking { get; set; }
    public bool IsDraft { get; set; }
    public bool SentToParent { get; set; }
    public bool IsVisibleToParent { get; set; }
    public DateTime? SentToParentAt { get; set; }
    public List<DailyFollowupSubjectDto> Subjects { get; set; } = new();
    public List<DailyFollowupActivityType> Activities { get; set; } = new();
    public List<DailyFollowupMealDto> Meals { get; set; } = new();
}
