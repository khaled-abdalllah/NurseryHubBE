using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class StudentApplication : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxChildFullNameLength = 256;
    public const int MaxParentFullNameLength = 256;
    public const int MaxPhoneNumberLength = 32;
    public const int MaxEmailLength = 256;
    public const int MaxNotesLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid NurseryBranchId { get; private set; }
    public Guid? RequestedGradeCategoryId { get; private set; }
    public string ChildFullName { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public string Gender { get; private set; } = null!;
    public string ParentFullName { get; private set; } = null!;
    public string ParentPhoneNumber { get; private set; } = null!;
    public string? SecondaryPhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Notes { get; private set; }
    public ApplicationStatus Status { get; private set; }

    protected StudentApplication()
    {
        ChildFullName = string.Empty;
        Gender = string.Empty;
        ParentFullName = string.Empty;
        ParentPhoneNumber = string.Empty;
        Status = ApplicationStatus.Pending;
    }

    public StudentApplication(
        Guid id,
        Guid? tenantId,
        Guid nurseryBranchId,
        Guid? requestedGradeCategoryId,
        string childFullName,
        DateOnly birthDate,
        string gender,
        string parentFullName,
        string parentPhoneNumber,
        string? secondaryPhoneNumber,
        string? email,
        string? notes,
        ApplicationStatus status = ApplicationStatus.Pending) : base(id)
    {
        TenantId = tenantId;
        NurseryBranchId = Check.NotNull(nurseryBranchId, nameof(nurseryBranchId));
        RequestedGradeCategoryId = requestedGradeCategoryId;
        SetChildFullName(childFullName);
        SetBirthDate(birthDate);
        SetGender(gender);
        SetParentFullName(parentFullName);
        SetParentPhoneNumber(parentPhoneNumber);
        SetSecondaryPhoneNumber(secondaryPhoneNumber);
        SetEmail(email);
        SetNotes(notes);
        Status = status;
    }

    public void SetNurseryBranch(Guid nurseryBranchId)
    {
        NurseryBranchId = Check.NotNull(nurseryBranchId, nameof(nurseryBranchId));
    }

    public void SetRequestedGradeCategory(Guid? requestedGradeCategoryId)
    {
        RequestedGradeCategoryId = requestedGradeCategoryId;
    }

    public void SetChildFullName(string childFullName)
    {
        ChildFullName = Check.NotNullOrWhiteSpace(childFullName, nameof(childFullName), MaxChildFullNameLength);
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

    public void SetParentFullName(string parentFullName)
    {
        ParentFullName = Check.NotNullOrWhiteSpace(parentFullName, nameof(parentFullName), MaxParentFullNameLength);
    }

    public void SetParentPhoneNumber(string parentPhoneNumber)
    {
        ParentPhoneNumber = Check.NotNullOrWhiteSpace(parentPhoneNumber, nameof(parentPhoneNumber), MaxPhoneNumberLength);
    }

    public void SetSecondaryPhoneNumber(string? secondaryPhoneNumber)
    {
        SecondaryPhoneNumber = string.IsNullOrWhiteSpace(secondaryPhoneNumber)
            ? null
            : Check.Length(secondaryPhoneNumber.Trim(), nameof(secondaryPhoneNumber), MaxPhoneNumberLength);
    }

    public void SetEmail(string? email)
    {
        Email = string.IsNullOrWhiteSpace(email)
            ? null
            : Check.Length(email.Trim(), nameof(email), MaxEmailLength);
    }

    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : Check.Length(notes.Trim(), nameof(notes), MaxNotesLength);
    }

    public void SetStatus(ApplicationStatus status)
    {
        Status = status;
    }
}
