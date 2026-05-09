using FluentValidation;

namespace NurseryHub.Nurseries;

public class ChangeStudentApplicationStatusDtoValidator : AbstractValidator<ChangeStudentApplicationStatusDto>
{
    public ChangeStudentApplicationStatusDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
