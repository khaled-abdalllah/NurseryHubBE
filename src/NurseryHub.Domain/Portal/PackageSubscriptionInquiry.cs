using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace NurseryHub.Portal;

public class PackageSubscriptionInquiry : FullAuditedEntity<Guid>
{
    public SubscriptionPackageTier PackageTier { get; private set; }
    public string NurseryName { get; private set; } = null!;
    public string ContactName { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Message { get; private set; }

    protected PackageSubscriptionInquiry()
    {
        NurseryName = string.Empty;
        ContactName = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
    }

    public PackageSubscriptionInquiry(
        Guid id,
        SubscriptionPackageTier packageTier,
        string nurseryName,
        string contactName,
        string phoneNumber,
        string email,
        string? message) : base(id)
    {
        SetPackageTier(packageTier);
        SetNurseryName(nurseryName);
        SetContactName(contactName);
        SetPhoneNumber(phoneNumber);
        SetEmail(email);
        SetMessage(message);
    }

    public void SetPackageTier(SubscriptionPackageTier packageTier)
    {
        if (!Enum.IsDefined(packageTier))
        {
            throw new BusinessException("NurseryHub:PackageSubscription:InvalidTier");
        }

        PackageTier = packageTier;
    }

    public void SetNurseryName(string nurseryName)
    {
        NurseryName = Check.NotNullOrWhiteSpace(nurseryName, nameof(nurseryName), PackageSubscriptionInquiryConsts.MaxNurseryNameLength);
    }

    public void SetContactName(string contactName)
    {
        ContactName = Check.NotNullOrWhiteSpace(contactName, nameof(contactName), PackageSubscriptionInquiryConsts.MaxContactNameLength);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = Check.NotNullOrWhiteSpace(phoneNumber, nameof(phoneNumber), PackageSubscriptionInquiryConsts.MaxPhoneNumberLength);
    }

    public void SetEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(email, nameof(email), PackageSubscriptionInquiryConsts.MaxEmailLength);
    }

    public void SetMessage(string? message)
    {
        Message = string.IsNullOrWhiteSpace(message)
            ? null
            : Check.Length(message, nameof(message), PackageSubscriptionInquiryConsts.MaxMessageLength);
    }
}
