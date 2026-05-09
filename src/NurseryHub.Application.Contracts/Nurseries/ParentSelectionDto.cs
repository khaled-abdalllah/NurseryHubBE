using System;

namespace NurseryHub.Nurseries;

public class ParentSelectionDto
{
    public Guid ParentId { get; set; }
    public string ParentName { get; set; } = null!;
    public string StudentName { get; set; } = null!;
    public string? ClassName { get; set; }
    public string? GradeCategoryName { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid BranchId { get; set; }
    public Guid StudentId { get; set; }
}
