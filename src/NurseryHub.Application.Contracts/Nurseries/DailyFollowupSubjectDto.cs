using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class DailyFollowupSubjectDto
{
    [Required]
    [MaxLength(128)]
    public string SubjectName { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string SubjectIcon { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int SortOrder { get; set; }
}
