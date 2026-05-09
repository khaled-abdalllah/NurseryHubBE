using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class StudentDto : FullAuditedEntityDto<Guid>
{
    public Guid NurseryBranchId { get; set; }
    public Guid? NurseryClassId { get; set; }
    public string? NurseryClassName { get; set; }
    /// <summary>FK to parent contact (father/mother details).</summary>
    public Guid ParentId { get; set; }
    public string FullName { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public string Gender { get; set; } = null!;
    public string? BloodType { get; set; }
    public string? Religion { get; set; }
    public string? HomeAddress { get; set; }
    public decimal? WeightKg { get; set; }
    public string FatherName { get; set; } = null!;
    public string FatherIdentityNumber { get; set; } = null!;
    public string FatherPhoneNumber { get; set; } = null!;
    public string MotherName { get; set; } = null!;
    public string MotherIdentityNumber { get; set; } = null!;
    public string MotherPhoneNumber { get; set; } = null!;
    public string EmergencyContactNumber { get; set; } = null!;
    public DateOnly EnrollmentDate { get; set; }
    public string? HealthNotes { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? ToiletTrainingStatus { get; set; }
    public bool AttendsSunday { get; set; }
    public bool AttendsMonday { get; set; }
    public bool AttendsTuesday { get; set; }
    public bool AttendsWednesday { get; set; }
    public bool AttendsThursday { get; set; }
    public bool AttendsFriday { get; set; }
    public bool AttendsSaturday { get; set; }
    public string? MedicalNotes { get; set; }
    public string? AllergyNotes { get; set; }
    public string? ProfileImageFileName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
}
