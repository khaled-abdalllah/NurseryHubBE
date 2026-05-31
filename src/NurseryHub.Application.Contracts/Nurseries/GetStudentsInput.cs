using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GetStudentsInput : PagedAndSortedResultRequestDto
{
    public Guid? NurseryBranchId { get; set; }
    public Guid? NurseryClassId { get; set; }
    public Guid? GradeCategoryId { get; set; }
    public bool? IsActive { get; set; }
    /// <summary>When true, only students with no nursery class assigned.</summary>
    public bool? WithoutNurseryClass { get; set; }
    public string? Religion { get; set; }
    public DateOnly? EnrollmentDateFrom { get; set; }
    public DateOnly? EnrollmentDateTo { get; set; }
    public string? Filter { get; set; }
}
