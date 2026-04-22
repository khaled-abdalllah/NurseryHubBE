using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class NurseryBranch : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;
    public const int MaxPhoneNumberLength = 32;
    public const int MaxAddressLength = 512;
    public const int MaxFacebookLinkLength = 1024;

    public Guid? TenantId { get; private set; }
    public Guid NurseryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string? AddressLine { get; private set; }
    public bool IsTransportationIncluded { get; private set; }
    public bool IsMealsIncluded { get; private set; }
    public Guid GovernorateId { get; private set; }
    public Guid CityId { get; private set; }
    public string? FacebookLink { get; private set; }
    public bool IsActive { get; private set; }

    protected NurseryBranch()
    {
        Name = string.Empty;
        PhoneNumber = string.Empty;
    }

    public NurseryBranch(
        Guid id,
        Guid? tenantId,
        Guid nurseryId,
        string name,
        string phoneNumber,
        Guid governorateId,
        Guid cityId,
        bool isTransportationIncluded,
        bool isMealsIncluded,
        string? addressLine = null,
        string? facebookLink = null,
        bool isActive = true) : base(id)
    {
        TenantId = tenantId;
        NurseryId = nurseryId;
        GovernorateId = governorateId;
        CityId = cityId;
        IsTransportationIncluded = isTransportationIncluded;
        IsMealsIncluded = isMealsIncluded;
        SetName(name);
        SetPhoneNumber(phoneNumber);
        SetAddressLine(addressLine);
        SetFacebookLink(facebookLink);
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

    public void SetAddressLine(string? addressLine)
    {
        AddressLine = Check.Length(addressLine, nameof(addressLine), MaxAddressLength);
    }

    public void SetFacebookLink(string? facebookLink)
    {
        FacebookLink = Check.Length(facebookLink, nameof(facebookLink), MaxFacebookLinkLength);
    }

    public void SetTransportationIncluded(bool isTransportationIncluded)
    {
        IsTransportationIncluded = isTransportationIncluded;
    }

    public void SetMealsIncluded(bool isMealsIncluded)
    {
        IsMealsIncluded = isMealsIncluded;
    }

    public void SetLocation(Guid governorateId, Guid cityId)
    {
        GovernorateId = governorateId;
        CityId = cityId;
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
