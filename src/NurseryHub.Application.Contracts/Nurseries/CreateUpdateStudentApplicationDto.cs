using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateStudentApplicationDto
{
    [Required]
    public Guid NurseryBranchId { get; set; }

    public Guid? RequestedGradeCategoryId { get; set; }

    [Required]
    [StringLength(StudentApplicationConsts.MaxChildFullNameLength)]
    public string ChildFullName { get; set; } = null!;

    [Required]
    public DateOnly BirthDate { get; set; }

    [Required]
    [StringLength(16)]
    public string Gender { get; set; } = null!;

    [Required]
    [StringLength(StudentApplicationConsts.MaxParentFullNameLength)]
    public string ParentFullName { get; set; } = null!;

    [Required]
    [StringLength(StudentApplicationConsts.MaxPhoneNumberLength)]
    public string ParentPhoneNumber { get; set; } = null!;

    [StringLength(StudentApplicationConsts.MaxPhoneNumberLength)]
    public string? SecondaryPhoneNumber { get; set; }

    [StringLength(StudentApplicationConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [StringLength(StudentApplicationConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
}
