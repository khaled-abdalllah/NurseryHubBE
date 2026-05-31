using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;

namespace NurseryHub.Nurseries;

/// <summary>
/// Server-side validation for student create/update. Messages are Arabic for portal UX.
/// </summary>
public class CreateUpdateStudentDtoValidator : AbstractValidator<CreateUpdateStudentDto>
{
    /// <summary>Arabic + English names (no digits); length enforced separately.</summary>
    public const string ArabicEnglishNamePattern = @"^[\u0600-\u06FFa-zA-Z\s]{2,100}$";

    /// <summary>Safe text for notes (Arabic, English, digits, limited punctuation).</summary>
    public const string SafeNotesPattern = @"^[\u0600-\u06FFa-zA-Z0-9\s.,!?()\-_%@]*$";

    /// <summary>Egyptian mobile (+20 / 0 prefix optional).</summary>
    public const string EgyptianPhonePattern = @"^(\+20|0)?1[0125][0-9]{8}$";

    public const string NationalIdPattern = @"^\d{14}$";

    public const string WeightPattern = @"^\d{1,2}(\.\d{1,2})?$";

    private static readonly string[] AllowedBloodTypes =
        ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    private static readonly string[] AllowedGenders = ["Male", "Female"];

    private static readonly Regex NameRegex = new(ArabicEnglishNamePattern, RegexOptions.Compiled);
    private static readonly Regex NotesRegex = new(SafeNotesPattern, RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(EgyptianPhonePattern, RegexOptions.Compiled);
    private static readonly Regex NationalIdRegex = new(NationalIdPattern, RegexOptions.Compiled);
    private static readonly Regex WeightRegex = new(WeightPattern, RegexOptions.Compiled);

    public CreateUpdateStudentDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.NurseryBranchId)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .MaximumLength(StudentConsts.MaxPersonNameLength)
            .WithMessage("الحد الأقصى للطول هو 100 حرف")
            .MinimumLength(2)
            .WithMessage("الحد الأدنى للطول حرفان")
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

        RuleFor(x => x.BloodType)
            .Must(v => string.IsNullOrWhiteSpace(v) || AllowedBloodTypes.Contains(v.Trim()))
            .WithMessage("قيمة غير صالحة");

        RuleFor(x => x.Religion)
            .MaximumLength(StudentConsts.MaxReligionLength)
            .WithMessage("القيمة المدخلة أطول من الحد المسموح")
            .Must(v => string.IsNullOrWhiteSpace(v) || NotesRegex.IsMatch(v.Trim()))
            .WithMessage("يحتوي الحقل على أحرف غير مسموحة");

        RuleFor(x => x.HomeAddress)
            .MaximumLength(StudentConsts.MaxHomeAddressLength)
            .WithMessage("القيمة المدخلة أطول من الحد المسموح")
            .Must(v => string.IsNullOrWhiteSpace(v) || NotesRegex.IsMatch(v.Trim()))
            .WithMessage("يحتوي الحقل على أحرف غير مسموحة");

        RuleFor(x => x.WeightKg)
            .Must(BeValidWeight)
            .WithMessage("قيمة الوزن غير صالحة")
            .When(x => x.WeightKg.HasValue);

        RuleFor(x => x.FatherName)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .MaximumLength(StudentConsts.MaxPersonNameLength)
            .WithMessage("الحد الأقصى للطول هو 100 حرف")
            .MinimumLength(2)
            .WithMessage("الحد الأدنى للطول حرفان")
            .Must(v => string.IsNullOrWhiteSpace(v) || NameRegex.IsMatch(v.Trim()))
            .WithMessage("يسمح بالحروف العربية والإنجليزية فقط");

        RuleFor(x => x.FatherIdentityNumber)
            .MaximumLength(14)
            .WithMessage("الرقم القومي يجب أن يكون 14 رقم")
            .Must(v => string.IsNullOrWhiteSpace(v) || NationalIdRegex.IsMatch(v.Trim()))
            .WithMessage("الرقم القومي يجب أن يكون 14 رقم");

        RuleFor(x => x.FatherPhoneNumber)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .Must(v => !string.IsNullOrWhiteSpace(v) && PhoneRegex.IsMatch(v.Trim()))
            .WithMessage("رقم الهاتف غير صحيح");

        RuleFor(x => x.MotherName)
            .MaximumLength(StudentConsts.MaxPersonNameLength)
            .WithMessage("الحد الأقصى للطول هو 100 حرف")
            .Must(v => string.IsNullOrWhiteSpace(v) || (v.Trim().Length >= 2 && NameRegex.IsMatch(v.Trim())))
            .WithMessage("يسمح بالحروف العربية والإنجليزية فقط");

        RuleFor(x => x.MotherIdentityNumber)
            .MaximumLength(14)
            .WithMessage("الرقم القومي يجب أن يكون 14 رقم")
            .Must(v => string.IsNullOrWhiteSpace(v) || NationalIdRegex.IsMatch(v.Trim()))
            .WithMessage("الرقم القومي يجب أن يكون 14 رقم");

        RuleFor(x => x.MotherPhoneNumber)
            .Must(v => string.IsNullOrWhiteSpace(v) || PhoneRegex.IsMatch(v.Trim()))
            .WithMessage("رقم الهاتف غير صحيح");

        RuleFor(x => x.EmergencyContactNumber)
            .NotEmpty()
            .WithMessage("هذا الحقل مطلوب")
            .Must(v => !string.IsNullOrWhiteSpace(v) && PhoneRegex.IsMatch(v.Trim()))
            .WithMessage("رقم الهاتف غير صحيح");

        RuleFor(x => x.DietaryRestrictions)
            .MaximumLength(StudentConsts.MaxDietaryRestrictionsLength)
            .WithMessage("القيمة المدخلة أطول من الحد المسموح")
            .Must(v => string.IsNullOrWhiteSpace(v) || NotesRegex.IsMatch(v.Trim()))
            .WithMessage("يحتوي الحقل على أحرف غير مسموحة");

        RuleFor(x => x.HealthNotes)
            .MaximumLength(StudentConsts.MaxHealthNotesLength)
            .WithMessage("القيمة المدخلة أطول من الحد المسموح")
            .Must(v => string.IsNullOrWhiteSpace(v) || NotesRegex.IsMatch(v.Trim()))
            .WithMessage("يحتوي الحقل على أحرف غير مسموحة");

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(StudentConsts.MaxMedicalNotesLength)
            .WithMessage("القيمة المدخلة أطول من الحد المسموح")
            .Must(v => string.IsNullOrWhiteSpace(v) || NotesRegex.IsMatch(v.Trim()))
            .WithMessage("يحتوي الحقل على أحرف غير مسموحة");

        RuleFor(x => x.AllergyNotes)
            .MaximumLength(StudentConsts.MaxAllergyNotesLength)
            .WithMessage("القيمة المدخلة أطول من الحد المسموح")
            .Must(v => string.IsNullOrWhiteSpace(v) || NotesRegex.IsMatch(v.Trim()))
            .WithMessage("يحتوي الحقل على أحرف غير مسموحة");

        RuleFor(x => x.ToiletTrainingStatus)
            .Must(StudentFieldNormalizer.IsAllowedToiletTraining)
            .WithMessage("قيمة غير صالحة");

        RuleFor(x => x)
            .Must(HasAtLeastOneAttendanceDay)
            .WithMessage("يجب اختيار يوم حضور واحد على الأقل")
            .OverridePropertyName("AttendanceDays");

        When(x => x.CreateParentPortalAccount, () =>
        {
            RuleFor(x => x.ParentLoginUsernameSource)
                .NotNull()
                .WithMessage("هذا الحقل مطلوب");

            When(
                x => x.ParentLoginUsernameSource == ParentPortalLoginUsernameSource.MotherPhone,
                () =>
                {
                    RuleFor(x => x.MotherPhoneNumber)
                        .NotEmpty()
                        .WithMessage("هذا الحقل مطلوب")
                        .Must(p => !string.IsNullOrWhiteSpace(p) && PhoneRegex.IsMatch(p.Trim()))
                        .WithMessage("رقم الهاتف غير صحيح");
                });
        });
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

    private static bool BeValidWeight(decimal? weight)
    {
        if (!weight.HasValue)
        {
            return true;
        }

        var s = weight.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!WeightRegex.IsMatch(s))
        {
            return false;
        }

        return weight.Value is >= 1m and <= 80m;
    }

    private static bool HasAtLeastOneAttendanceDay(CreateUpdateStudentDto x)
    {
        return x.AttendsSunday || x.AttendsMonday || x.AttendsTuesday || x.AttendsWednesday ||
               x.AttendsThursday || x.AttendsFriday || x.AttendsSaturday;
    }
}
