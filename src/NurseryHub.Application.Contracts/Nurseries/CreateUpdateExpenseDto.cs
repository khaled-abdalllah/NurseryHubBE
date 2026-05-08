using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateExpenseDto : IValidatableObject
{
    [Required]
    public Guid NurseryBranchId { get; set; }

    [Required]
    [StringLength(ExpenseConsts.MaxTitleLength)]
    public string Title { get; set; } = null!;

    [Required]
    public ExpenseCategory ExpenseCategory { get; set; }

    [StringLength(ExpenseConsts.MaxCustomCategoryLength)]
    public string? CustomExpenseCategory { get; set; }

    [StringLength(ExpenseConsts.MaxVendorNameLength)]
    public string? VendorName { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    public ExpensePaymentStatus PaymentStatus { get; set; } = ExpensePaymentStatus.Paid;

    [Required]
    public decimal Amount { get; set; }

    public decimal TaxAmount { get; set; }

    [Required]
    public DateTime ExpenseDate { get; set; }

    [StringLength(ExpenseConsts.MaxReceiptNumberLength)]
    public string? ReceiptNumber { get; set; }

    [StringLength(ExpenseConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public Guid? ReceiptFileId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NurseryBranchId == Guid.Empty)
        {
            yield return new ValidationResult("Nursery branch is required.", new[] { nameof(NurseryBranchId) });
        }

        if (Amount <= 0)
        {
            yield return new ValidationResult("Amount must be greater than zero.", new[] { nameof(Amount) });
        }

        if (ExpenseCategory == ExpenseCategory.Other && string.IsNullOrWhiteSpace(CustomExpenseCategory))
        {
            yield return new ValidationResult(
                "Custom expense category is required when category is Other.",
                new[] { nameof(CustomExpenseCategory) });
        }

        if (PaymentStatus != ExpensePaymentStatus.Paid)
        {
            yield return new ValidationResult("Expense payment status must be Paid.", new[] { nameof(PaymentStatus) });
        }
    }
}
