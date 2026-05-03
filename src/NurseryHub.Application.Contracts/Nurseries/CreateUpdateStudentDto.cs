using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateStudentDto
{
    [Required]
    public Guid NurseryBranchId { get; set; }

    public Guid? NurseryClassId { get; set; }

    [Required]
    [StringLength(StudentConsts.MaxFullNameLength)]
    public string FullName { get; set; } = null!;

    [Required]
    public DateOnly BirthDate { get; set; }

    [Required]
    [StringLength(16)]
    public string Gender { get; set; } = null!;

    [StringLength(StudentConsts.MaxBloodTypeLength)]
    public string? BloodType { get; set; }

    [StringLength(StudentConsts.MaxReligionLength)]
    public string? Religion { get; set; }

    [StringLength(StudentConsts.MaxHomeAddressLength)]
    public string? HomeAddress { get; set; }

    [Required]
    [StringLength(StudentConsts.MaxFullNameLength)]
    public string FatherName { get; set; } = null!;

    [Required]
    [StringLength(StudentConsts.MaxIdentityNumberLength)]
    public string FatherIdentityNumber { get; set; } = null!;

    [Required]
    [StringLength(StudentConsts.MaxPhoneNumberLength)]
    public string FatherPhoneNumber { get; set; } = null!;

    [Required]
    [StringLength(StudentConsts.MaxFullNameLength)]
    public string MotherName { get; set; } = null!;

    [Required]
    [StringLength(StudentConsts.MaxIdentityNumberLength)]
    public string MotherIdentityNumber { get; set; } = null!;

    [Required]
    [StringLength(StudentConsts.MaxPhoneNumberLength)]
    public string MotherPhoneNumber { get; set; } = null!;

    [Required]
    [StringLength(StudentConsts.MaxPhoneNumberLength)]
    public string EmergencyContactNumber { get; set; } = null!;

    [Required]
    public DateOnly EnrollmentDate { get; set; }

    [StringLength(StudentConsts.MaxHealthNotesLength)]
    public string? HealthNotes { get; set; }

    [StringLength(StudentConsts.MaxDietaryRestrictionsLength)]
    public string? DietaryRestrictions { get; set; }

    [StringLength(StudentConsts.MaxToiletTrainingStatusLength)]
    public string? ToiletTrainingStatus { get; set; }

    [Required]
    public bool AttendsSunday { get; set; }

    [Required]
    public bool AttendsMonday { get; set; }

    [Required]
    public bool AttendsTuesday { get; set; }

    [Required]
    public bool AttendsWednesday { get; set; }

    [Required]
    public bool AttendsThursday { get; set; }

    [Required]
    public bool AttendsFriday { get; set; }

    [Required]
    public bool AttendsSaturday { get; set; }

    [StringLength(StudentConsts.MaxMedicalNotesLength)]
    public string? MedicalNotes { get; set; }

    public bool IsActive { get; set; } = true;
}
