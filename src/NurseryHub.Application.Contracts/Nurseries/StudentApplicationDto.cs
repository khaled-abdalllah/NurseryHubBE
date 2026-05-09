using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class StudentApplicationDto : FullAuditedEntityDto<Guid>
{
    public Guid NurseryBranchId { get; set; }
    public Guid? RequestedGradeCategoryId { get; set; }
    public string? RequestedGradeName { get; set; }
    public string ChildFullName { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public string Gender { get; set; } = null!;
    public string ParentFullName { get; set; } = null!;
    public string ParentPhoneNumber { get; set; } = null!;
    public string? SecondaryPhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public ApplicationStatus Status { get; set; }
}
