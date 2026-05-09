using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class Student : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxFullNameLength = 256;
    public const int MaxPhoneNumberLength = 32;
    public const int MaxBloodTypeLength = 16;
    public const int MaxReligionLength = 64;
    public const int MaxHomeAddressLength = 512;
    public const int MaxToiletTrainingStatusLength = 64;
    public const int MaxDietaryRestrictionsLength = 500;
    public const int MaxHealthNotesLength = 1000;
    public const int MaxMedicalNotesLength = 1000;
    public const int MaxAllergyNotesLength = 1000;
    public const int MaxProfileImageFileNameLength = 256;

    public Guid? TenantId { get; private set; }
    public Guid NurseryBranchId { get; private set; }
    public Guid? NurseryClassId { get; private set; }
    /// <summary>
    /// FK to <see cref="ParentContact"/> (father/mother details).
    /// </summary>
    public Guid ParentId { get; private set; }
    public string FullName { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public string Gender { get; private set; } = null!;
    public string? BloodType { get; private set; }
    public string? Religion { get; private set; }
    public string? HomeAddress { get; private set; }
    public string EmergencyContactNumber { get; private set; } = null!;
    public DateOnly EnrollmentDate { get; private set; }
    public string? HealthNotes { get; private set; }
    public string? DietaryRestrictions { get; private set; }
    public string? ToiletTrainingStatus { get; private set; }
    public bool AttendsSunday { get; private set; }
    public bool AttendsMonday { get; private set; }
    public bool AttendsTuesday { get; private set; }
    public bool AttendsWednesday { get; private set; }
    public bool AttendsThursday { get; private set; }
    public bool AttendsFriday { get; private set; }
    public bool AttendsSaturday { get; private set; }
    public string? MedicalNotes { get; private set; }
    public string? AllergyNotes { get; private set; }
    /// <summary>Optional weight in kilograms (e.g. 12.5).</summary>
    public decimal? WeightKg { get; private set; }
    public string? ProfileImageFileName { get; private set; }
    public bool IsActive { get; private set; }

    public virtual ParentContact Parent { get; protected set; } = null!;

    protected Student()
    {
        FullName = string.Empty;
        Gender = string.Empty;
        EmergencyContactNumber = string.Empty;
    }

    public Student(
        Guid id,
        Guid? tenantId,
        Guid nurseryBranchId,
        Guid parentId,
        string fullName,
        DateOnly birthDate,
        string gender,
        string? bloodType,
        string? religion,
        string? homeAddress,
        string emergencyContactNumber,
        DateOnly enrollmentDate,
        string? healthNotes,
        string? dietaryRestrictions,
        string? toiletTrainingStatus,
        bool attendsSunday,
        bool attendsMonday,
        bool attendsTuesday,
        bool attendsWednesday,
        bool attendsThursday,
        bool attendsFriday,
        bool attendsSaturday,
        string? medicalNotes,
        string? allergyNotes,
        decimal? weightKg,
        string? profileImageFileName,
        bool isActive = true,
        Guid? nurseryClassId = null) : base(id)
    {
        TenantId = tenantId;
        NurseryBranchId = nurseryBranchId;
        SetClass(nurseryClassId);
        SetParentId(parentId);
        SetFullName(fullName);
        SetBirthDate(birthDate);
        SetGender(gender);
        SetBloodType(bloodType);
        SetReligion(religion);
        SetHomeAddress(homeAddress);
        SetEmergencyContactNumber(emergencyContactNumber);
        SetEnrollmentDate(enrollmentDate);
        SetHealthNotes(healthNotes);
        SetDietaryRestrictions(dietaryRestrictions);
        SetToiletTrainingStatus(toiletTrainingStatus);
        SetAttendingDays(attendsSunday, attendsMonday, attendsTuesday, attendsWednesday, attendsThursday, attendsFriday, attendsSaturday);
        SetMedicalNotes(medicalNotes);
        SetAllergyNotes(allergyNotes);
        SetWeightKg(weightKg);
        SetProfileImageFileName(profileImageFileName);
        IsActive = isActive;
    }

    public void SetParentId(Guid parentId)
    {
        if (parentId == Guid.Empty)
        {
            throw new ArgumentException("Parent id cannot be empty.", nameof(parentId));
        }

        ParentId = parentId;
    }

    public void SetClass(Guid? nurseryClassId)
    {
        NurseryClassId = nurseryClassId;
    }

    public void SetFullName(string fullName)
    {
        FullName = Check.NotNullOrWhiteSpace(fullName, nameof(fullName), MaxFullNameLength);
    }

    public void SetBirthDate(DateOnly birthDate)
    {
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
        }

        BirthDate = birthDate;
    }

    public void SetGender(string gender)
    {
        Gender = Check.NotNullOrWhiteSpace(gender, nameof(gender), 16);
    }

    public void SetBloodType(string? bloodType)
    {
        BloodType = Check.Length(bloodType, nameof(bloodType), MaxBloodTypeLength);
    }

    public void SetReligion(string? religion)
    {
        Religion = Check.Length(religion, nameof(religion), MaxReligionLength);
    }

    public void SetHomeAddress(string? homeAddress)
    {
        HomeAddress = Check.Length(homeAddress, nameof(homeAddress), MaxHomeAddressLength);
    }

    public void SetEmergencyContactNumber(string emergencyContactNumber)
    {
        EmergencyContactNumber = Check.NotNullOrWhiteSpace(
            emergencyContactNumber,
            nameof(emergencyContactNumber),
            MaxPhoneNumberLength);
    }

    public void SetEnrollmentDate(DateOnly enrollmentDate)
    {
        EnrollmentDate = enrollmentDate;
    }

    public void SetHealthNotes(string? healthNotes)
    {
        HealthNotes = Check.Length(healthNotes, nameof(healthNotes), MaxHealthNotesLength);
    }

    public void SetDietaryRestrictions(string? dietaryRestrictions)
    {
        DietaryRestrictions = Check.Length(dietaryRestrictions, nameof(dietaryRestrictions), MaxDietaryRestrictionsLength);
    }

    public void SetToiletTrainingStatus(string? toiletTrainingStatus)
    {
        ToiletTrainingStatus = Check.Length(
            toiletTrainingStatus,
            nameof(toiletTrainingStatus),
            MaxToiletTrainingStatusLength);
    }

    public void SetAttendingDays(
        bool attendsSunday,
        bool attendsMonday,
        bool attendsTuesday,
        bool attendsWednesday,
        bool attendsThursday,
        bool attendsFriday,
        bool attendsSaturday)
    {
        AttendsSunday = attendsSunday;
        AttendsMonday = attendsMonday;
        AttendsTuesday = attendsTuesday;
        AttendsWednesday = attendsWednesday;
        AttendsThursday = attendsThursday;
        AttendsFriday = attendsFriday;
        AttendsSaturday = attendsSaturday;
    }

    public void SetMedicalNotes(string? medicalNotes)
    {
        MedicalNotes = Check.Length(medicalNotes, nameof(medicalNotes), MaxMedicalNotesLength);
    }

    public void SetAllergyNotes(string? allergyNotes)
    {
        AllergyNotes = Check.Length(allergyNotes, nameof(allergyNotes), MaxAllergyNotesLength);
    }

    /// <summary>Sets weight in kg; null clears. Allowed range 1–80 when set.</summary>
    public void SetWeightKg(decimal? weightKg)
    {
        if (weightKg.HasValue)
        {
            if (weightKg.Value < 1m || weightKg.Value > 80m)
            {
                throw new ArgumentOutOfRangeException(nameof(weightKg), weightKg, "Weight must be between 1 and 80 kg.");
            }
        }

        WeightKg = weightKg;
    }

    public void SetProfileImageFileName(string? profileImageFileName)
    {
        ProfileImageFileName = Check.Length(
            profileImageFileName,
            nameof(profileImageFileName),
            MaxProfileImageFileNameLength);
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
