using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class GetDailyFollowupBookInput
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid NurseryBranchId { get; set; }

    [Required]
    public DateOnly Date { get; set; }
}
