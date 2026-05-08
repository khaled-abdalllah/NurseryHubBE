using System.ComponentModel.DataAnnotations;

using System;

namespace NurseryHub.Nurseries;

public class CreateUpdateGradeCategoryDto
{
    [Required]
    public Guid NurseryBranchId { get; set; }

    [Required]
    [StringLength(GradeCategoryConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(GradeCategoryConsts.MaxIconLength)]
    public string Icon { get; set; } = "grade";

    [Required]
    [StringLength(GradeCategoryConsts.MaxColorTokenLength)]
    public string ColorToken { get; set; } = "secondary-fixed";

    [StringLength(GradeCategoryConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
