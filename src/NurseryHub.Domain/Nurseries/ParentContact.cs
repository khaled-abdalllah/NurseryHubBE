using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

/// <summary>
/// Father and mother contact details for one or more students (typically one student per row).
/// </summary>
public class ParentContact : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxFullNameLength = 256;
    public const int MaxIdentityNumberLength = 64;
    public const int MaxPhoneNumberLength = 32;

    public Guid? TenantId { get; private set; }
    public string FatherName { get; private set; } = null!;
    public string FatherIdentityNumber { get; private set; } = null!;
    public string FatherPhoneNumber { get; private set; } = null!;
    public string MotherName { get; private set; } = null!;
    public string MotherIdentityNumber { get; private set; } = null!;
    public string MotherPhoneNumber { get; private set; } = null!;

    protected ParentContact()
    {
        FatherName = string.Empty;
        FatherIdentityNumber = string.Empty;
        FatherPhoneNumber = string.Empty;
        MotherName = string.Empty;
        MotherIdentityNumber = string.Empty;
        MotherPhoneNumber = string.Empty;
    }

    public ParentContact(
        Guid id,
        Guid? tenantId,
        string fatherName,
        string fatherIdentityNumber,
        string fatherPhoneNumber,
        string motherName,
        string motherIdentityNumber,
        string motherPhoneNumber) : base(id)
    {
        TenantId = tenantId;
        SetFatherInfo(fatherName, fatherIdentityNumber, fatherPhoneNumber);
        SetMotherInfo(motherName, motherIdentityNumber, motherPhoneNumber);
    }

    public void SetFatherInfo(string fatherName, string? fatherIdentityNumber, string fatherPhoneNumber)
    {
        FatherName = Check.NotNullOrWhiteSpace(fatherName, nameof(fatherName), MaxFullNameLength);
        FatherIdentityNumber = NormalizeOptionalIdentity(fatherIdentityNumber);
        FatherPhoneNumber = Check.NotNullOrWhiteSpace(fatherPhoneNumber, nameof(fatherPhoneNumber), MaxPhoneNumberLength);
    }

    public void SetMotherInfo(string? motherName, string? motherIdentityNumber, string? motherPhoneNumber)
    {
        MotherName = NormalizeOptionalPersonName(motherName);
        MotherIdentityNumber = NormalizeOptionalIdentity(motherIdentityNumber);
        MotherPhoneNumber = NormalizeOptionalPhone(motherPhoneNumber);
    }

    private static string NormalizeOptionalIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Check.Length(value.Trim(), nameof(value), MaxIdentityNumberLength);
    }

    private static string NormalizeOptionalPersonName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Check.Length(value.Trim(), nameof(value), MaxFullNameLength);
    }

    private static string NormalizeOptionalPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Check.Length(value.Trim(), nameof(value), MaxPhoneNumberLength);
    }
}
