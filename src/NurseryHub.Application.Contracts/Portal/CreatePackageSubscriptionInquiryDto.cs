using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Portal;

public class CreatePackageSubscriptionInquiryDto
{
    [Required]
    public SubscriptionPackageTier PackageTier { get; set; }

    [Required]
    [StringLength(PackageSubscriptionInquiryConsts.MaxNurseryNameLength)]
    public string NurseryName { get; set; } = string.Empty;

    [Required]
    [StringLength(PackageSubscriptionInquiryConsts.MaxContactNameLength)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [StringLength(PackageSubscriptionInquiryConsts.MaxPhoneNumberLength)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(PackageSubscriptionInquiryConsts.MaxEmailLength)]
    public string Email { get; set; } = string.Empty;

    [StringLength(PackageSubscriptionInquiryConsts.MaxMessageLength)]
    public string? Message { get; set; }
}
