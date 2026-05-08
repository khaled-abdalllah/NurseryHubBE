using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class DailyFollowupMealDto
{
    [Required]
    public DailyFollowupMealType MealType { get; set; }

    [Required]
    public DailyFollowupMealStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
