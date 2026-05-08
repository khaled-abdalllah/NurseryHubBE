using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateNurseryClassDto
{
    [Required]
    public Guid NurseryBranchId { get; set; }

    [Required]
    public Guid GradeCategoryId { get; set; }

    [Required]
    [StringLength(NurseryClassConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [StringLength(NurseryClassConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [Range(1, 500)]
    public int Capacity { get; set; }

    public int? MinAgeInMonths { get; set; }

    public int? MaxAgeInMonths { get; set; }

    public bool IsActive { get; set; } = true;
}
