using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace NurseryHub.Portal;

public class CreatePackageSubscriptionInquiryDtoValidator : AbstractValidator<CreatePackageSubscriptionInquiryDto>
{
    public const string EgyptianPhonePattern = @"^(\+20|0)?1[0125][0-9]{8}$";
    private static readonly Regex PhoneRegex = new(EgyptianPhonePattern, RegexOptions.Compiled);

    public CreatePackageSubscriptionInquiryDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PackageTier)
            .Must(t => Enum.IsDefined(typeof(SubscriptionPackageTier), (int)t))
            .WithMessage("Invalid package selection.");

        RuleFor(x => x.NurseryName)
            .NotEmpty()
            .MaximumLength(PackageSubscriptionInquiryConsts.MaxNurseryNameLength);

        RuleFor(x => x.ContactName)
            .NotEmpty()
            .MaximumLength(PackageSubscriptionInquiryConsts.MaxContactNameLength);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Must(v => !string.IsNullOrWhiteSpace(v) && PhoneRegex.IsMatch(v.Trim()))
            .WithMessage("Phone number is not valid.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(PackageSubscriptionInquiryConsts.MaxEmailLength);

        RuleFor(x => x.Message)
            .MaximumLength(PackageSubscriptionInquiryConsts.MaxMessageLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Message));
    }
}
