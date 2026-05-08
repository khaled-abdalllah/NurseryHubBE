using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdatePaymentDto : IValidatableObject
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid GradeId { get; set; }

    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentPurpose PaymentPurpose { get; set; }

    [StringLength(PaymentConsts.MaxCustomPurposeLength)]
    public string? CustomPaymentPurpose { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [StringLength(PaymentConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Amount <= 0)
        {
            yield return new ValidationResult(
                "Amount must be greater than zero.",
                new[] { nameof(Amount) });
        }

        if (PaymentPurpose == PaymentPurpose.Other && string.IsNullOrWhiteSpace(CustomPaymentPurpose))
        {
            yield return new ValidationResult(
                "Custom payment purpose is required when purpose is Other.",
                new[] { nameof(CustomPaymentPurpose) });
        }
    }
}
