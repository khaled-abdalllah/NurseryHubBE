using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class Student : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxFullNameLength = 256;
    public const int MaxIdentityNumberLength = 64;
    public const int MaxPhoneNumberLength = 32;
    public const int MaxBloodTypeLength = 16;
    public const int MaxReligionLength = 64;
    public const int MaxHomeAddressLength = 512;
    public const int MaxToiletTrainingStatusLength = 64;
    public const int MaxDietaryRestrictionsLength = 1000;
    public const int MaxHealthNotesLength = 2000;
    public const int MaxMedicalNotesLength = 2000;
    public const int MaxProfileImageFileNameLength = 256;

    public Guid? TenantId { get; private set; }
    public Guid NurseryBranchId { get; private set; }
    public Guid? NurseryClassId { get; private set; }
    public string FullName { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public string Gender { get; private set; } = null!;
    public string? BloodType { get; private set; }
    public string? Religion { get; private set; }
    public string? HomeAddress { get; private set; }
    public string FatherName { get; private set; } = null!;
    public string FatherIdentityNumber { get; private set; } = null!;
    public string FatherPhoneNumber { get; private set; } = null!;
    public string MotherName { get; private set; } = null!;
    public string MotherIdentityNumber { get; private set; } = null!;
    public string MotherPhoneNumber { get; private set; } = null!;
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
    public string? ProfileImageFileName { get; private set; }
    public bool IsActive { get; private set; }

    protected Student()
    {
        FullName = string.Empty;
        Gender = string.Empty;
        FatherName = string.Empty;
        FatherIdentityNumber = string.Empty;
        FatherPhoneNumber = string.Empty;
        MotherName = string.Empty;
        MotherIdentityNumber = string.Empty;
        MotherPhoneNumber = string.Empty;
        EmergencyContactNumber = string.Empty;
    }

    public Student(
        Guid id,
        Guid? tenantId,
        Guid nurseryBranchId,
        string fullName,
        DateOnly birthDate,
        string gender,
        string? bloodType,
        string? religion,
        string? homeAddress,
        string fatherName,
        string fatherIdentityNumber,
        string fatherPhoneNumber,
        string motherName,
        string motherIdentityNumber,
        string motherPhoneNumber,
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
        string? profileImageFileName,
        bool isActive = true,
        Guid? nurseryClassId = null) : base(id)
    {
        TenantId = tenantId;
        NurseryBranchId = nurseryBranchId;
        SetClass(nurseryClassId);
        SetFullName(fullName);
        SetBirthDate(birthDate);
        SetGender(gender);
        SetBloodType(bloodType);
        SetReligion(religion);
        SetHomeAddress(homeAddress);
        SetFatherInfo(fatherName, fatherIdentityNumber, fatherPhoneNumber);
        SetMotherInfo(motherName, motherIdentityNumber, motherPhoneNumber);
        SetEmergencyContactNumber(emergencyContactNumber);
        SetEnrollmentDate(enrollmentDate);
        SetHealthNotes(healthNotes);
        SetDietaryRestrictions(dietaryRestrictions);
        SetToiletTrainingStatus(toiletTrainingStatus);
        SetAttendingDays(attendsSunday, attendsMonday, attendsTuesday, attendsWednesday, attendsThursday, attendsFriday, attendsSaturday);
        SetMedicalNotes(medicalNotes);
        SetProfileImageFileName(profileImageFileName);
        IsActive = isActive;
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

    public void SetFatherInfo(string fatherName, string fatherIdentityNumber, string fatherPhoneNumber)
    {
        FatherName = Check.NotNullOrWhiteSpace(fatherName, nameof(fatherName), MaxFullNameLength);
        FatherIdentityNumber = Check.NotNullOrWhiteSpace(
            fatherIdentityNumber,
            nameof(fatherIdentityNumber),
            MaxIdentityNumberLength);
        FatherPhoneNumber = Check.NotNullOrWhiteSpace(fatherPhoneNumber, nameof(fatherPhoneNumber), MaxPhoneNumberLength);
    }

    public void SetMotherInfo(string motherName, string motherIdentityNumber, string motherPhoneNumber)
    {
        MotherName = Check.NotNullOrWhiteSpace(motherName, nameof(motherName), MaxFullNameLength);
        MotherIdentityNumber = Check.NotNullOrWhiteSpace(
            motherIdentityNumber,
            nameof(motherIdentityNumber),
            MaxIdentityNumberLength);
        MotherPhoneNumber = Check.NotNullOrWhiteSpace(motherPhoneNumber, nameof(motherPhoneNumber), MaxPhoneNumberLength);
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
        if (enrollmentDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Enrollment date cannot be in the future.", nameof(enrollmentDate));
        }

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
