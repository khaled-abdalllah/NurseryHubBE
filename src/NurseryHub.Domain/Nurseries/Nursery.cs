using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class Nursery : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;
    public const int MaxPhoneNumberLength = 32;
    public const int MaxEmailLength = 256;
    public const int MaxLogoContentTypeLength = 128;
    public const int MaxWebsiteUrlLength = 1024;

    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public byte[]? LogoData { get; private set; }
    public string? LogoContentType { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public bool IsActive { get; private set; }

    protected Nursery()
    {
        Name = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
    }

    public Nursery(
        Guid id,
        Guid? tenantId,
        string name,
        string phoneNumber,
        string email,
        string? websiteUrl = null,
        bool isActive = true) : base(id)
    {
        TenantId = tenantId;
        SetName(name);
        SetPhoneNumber(phoneNumber);
        SetEmail(email);
        SetWebsiteUrl(websiteUrl);
        IsActive = isActive;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), MaxNameLength);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = Check.NotNullOrWhiteSpace(phoneNumber, nameof(phoneNumber), MaxPhoneNumberLength);
    }

    public void SetEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(email, nameof(email), MaxEmailLength);
    }

    public void SetLogo(byte[] logoData, string contentType)
    {
        Check.NotNull(logoData, nameof(logoData));
        if (logoData.Length == 0)
        {
            throw new ArgumentException("Logo data must not be empty.", nameof(logoData));
        }

        LogoData = logoData;
        LogoContentType = Check.NotNullOrWhiteSpace(contentType, nameof(contentType), MaxLogoContentTypeLength);
    }

    public void ClearLogo()
    {
        LogoData = null;
        LogoContentType = null;
    }

    public bool HasLogo => LogoData is { Length: > 0 };

    public void SetWebsiteUrl(string? websiteUrl)
    {
        WebsiteUrl = Check.Length(websiteUrl, nameof(websiteUrl), MaxWebsiteUrlLength);
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
