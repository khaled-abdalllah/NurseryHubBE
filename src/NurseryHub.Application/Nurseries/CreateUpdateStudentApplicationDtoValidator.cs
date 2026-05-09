using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace NurseryHub.Nurseries;

public class CreateUpdateStudentApplicationDtoValidator : AbstractValidator<CreateUpdateStudentApplicationDto>
{
    public const string ArabicEnglishNamePattern = @"^[\u0600-\u06FFa-zA-Z\s]{2,100}$";
    public const string EgyptianPhonePattern = @"^(\+20|0)?1[0125][0-9]{8}$";
    public const string SafeNotesPattern = @"^[\u0600-\u06FFa-zA-Z0-9\s.,!?()\-_%@]*$";

    private static readonly string[] AllowedGenders = ["Male", "Female"];
    private static readonly Regex NameRegex = new(ArabicEnglishNamePattern, RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(EgyptianPhonePattern, RegexOptions.Compiled);
    private static readonly Regex NotesRegex = new(SafeNotesPattern, RegexOptions.Compiled);

    public CreateUpdateStudentApplicationDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.NurseryBranchId)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب");

        RuleFor(x => x.ChildFullName)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .MaximumLength(StudentApplicationConsts.MaxChildFullNameLength)
            .MinimumLength(2)
            .Must(v => string.IsNullOrWhiteSpace(v) || NameRegex.IsMatch(v.Trim()))
            .WithMessage("يسمح بالحروف العربية والإنجليزية فقط");

        RuleFor(x => x.BirthDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("لا يمكن اختيار تاريخ في المستقبل")
            .Must(BeAgeBetweenThreeMonthsAndTenYears)
            .WithMessage("العمر غير مسموح");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .Must(g => AllowedGenders.Contains(g))
            .WithMessage("قيمة غير صالحة");

        RuleFor(x => x.ParentFullName)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .MaximumLength(StudentApplicationConsts.MaxParentFullNameLength)
            .MinimumLength(2)
            .Must(v => string.IsNullOrWhiteSpace(v) || NameRegex.IsMatch(v.Trim()))
            .WithMessage("يسمح بالحروف العربية والإنجليزية فقط");

        RuleFor(x => x.ParentPhoneNumber)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .Must(v => !string.IsNullOrWhiteSpace(v) && PhoneRegex.IsMatch(v.Trim()))
            .WithMessage("رقم الهاتف غير صحيح");

        RuleFor(x => x.SecondaryPhoneNumber)
            .Must(v => string.IsNullOrWhiteSpace(v) || PhoneRegex.IsMatch(v.Trim()))
            .WithMessage("رقم الهاتف غير صحيح");

        RuleFor(x => x.Email)
            .MaximumLength(StudentApplicationConsts.MaxEmailLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("القيمة المدخلة أطول من الحد المسموح");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("البريد الإلكتروني غير صالح");

        RuleFor(x => x.Notes)
            .MaximumLength(StudentApplicationConsts.MaxNotesLength)
            .WithMessage("القيمة المدخلة أطول من الحد المسموح")
            .Must(v => string.IsNullOrWhiteSpace(v) || NotesRegex.IsMatch(v.Trim()))
            .WithMessage("يحتوي الحقل على أحرف غير مسموحة");
    }

    private static bool BeAgeBetweenThreeMonthsAndTenYears(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (birthDate > today)
        {
            return false;
        }

        var minBirth = today.AddYears(-10);
        var maxBirth = today.AddMonths(-3);
        return birthDate >= minBirth && birthDate <= maxBirth;
    }
}
