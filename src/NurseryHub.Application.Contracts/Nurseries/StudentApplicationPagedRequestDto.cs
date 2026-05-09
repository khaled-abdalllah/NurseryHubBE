using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class StudentApplicationPagedRequestDto : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid NurseryBranchId { get; set; }

    public string? ChildNameFilter { get; set; }
    public string? ParentPhoneFilter { get; set; }
    public ApplicationStatus? Status { get; set; }
    public Guid? RequestedGradeCategoryId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
