using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class SaveDailyFollowupBookInput
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid NurseryBranchId { get; set; }

    [Required]
    public DateOnly ReportDate { get; set; }

    [Required]
    public DailyFollowupMood OverallMood { get; set; }

    [Required]
    [MaxLength(2000)]
    public string TeacherNote { get; set; } = string.Empty;

    public bool SleptToday { get; set; }

    [MaxLength(64)]
    public string? SleepDuration { get; set; }

    public DailyFollowupNapMood? MoodAfterWaking { get; set; }

    public List<DailyFollowupSubjectDto> Subjects { get; set; } = new();
    public List<DailyFollowupActivityType> Activities { get; set; } = new();
    public List<DailyFollowupMealDto> Meals { get; set; } = new();
}
