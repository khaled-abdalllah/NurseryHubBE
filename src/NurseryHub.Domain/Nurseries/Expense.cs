using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class Expense : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxExpenseNumberLength = 64;
    public const int MaxTitleLength = 256;
    public const int MaxCustomCategoryLength = 128;
    public const int MaxVendorNameLength = 256;
    public const int MaxReceiptNumberLength = 64;
    public const int MaxNotesLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid NurseryBranchId { get; private set; }
    public string ExpenseNumber { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public ExpenseCategory ExpenseCategory { get; private set; }
    public string? CustomExpenseCategory { get; private set; }
    public string? VendorName { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public ExpensePaymentStatus PaymentStatus { get; private set; }
    public decimal Amount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime ExpenseDate { get; private set; }
    public string? ReceiptNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ReceiptFileId { get; private set; }

    protected Expense()
    {
    }

    public Expense(
        Guid id,
        Guid? tenantId,
        Guid nurseryBranchId,
        string expenseNumber,
        string title,
        ExpenseCategory expenseCategory,
        string? customExpenseCategory,
        string? vendorName,
        PaymentMethod paymentMethod,
        ExpensePaymentStatus paymentStatus,
        decimal amount,
        decimal taxAmount,
        DateTime expenseDate,
        string? receiptNumber,
        string? notes,
        Guid? receiptFileId) : base(id)
    {
        TenantId = tenantId;
        NurseryBranchId = Check.NotNull(nurseryBranchId, nameof(nurseryBranchId));
        SetExpenseNumber(expenseNumber);
        SetInformation(title, expenseCategory, customExpenseCategory, vendorName);
        SetPayment(paymentMethod, paymentStatus);
        SetAmounts(amount, taxAmount);
        SetExpenseDate(expenseDate);
        SetReceipt(receiptNumber, receiptFileId);
        SetNotes(notes);
    }

    public void SetExpenseNumber(string expenseNumber)
    {
        ExpenseNumber = Check.NotNullOrWhiteSpace(expenseNumber, nameof(expenseNumber), MaxExpenseNumberLength);
    }

    public void SetNurseryBranch(Guid nurseryBranchId)
    {
        NurseryBranchId = Check.NotNull(nurseryBranchId, nameof(nurseryBranchId));
    }

    public void SetInformation(string title, ExpenseCategory expenseCategory, string? customExpenseCategory, string? vendorName)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), MaxTitleLength);
        ExpenseCategory = expenseCategory;

        var normalizedCustomCategory = string.IsNullOrWhiteSpace(customExpenseCategory)
            ? null
            : customExpenseCategory.Trim();
        if (expenseCategory == ExpenseCategory.Other && normalizedCustomCategory.IsNullOrWhiteSpace())
        {
            throw new BusinessException("NurseryHub:Expenses:CustomCategoryRequired");
        }

        CustomExpenseCategory = Check.Length(normalizedCustomCategory, nameof(customExpenseCategory), MaxCustomCategoryLength);
        VendorName = Check.Length(vendorName?.Trim(), nameof(vendorName), MaxVendorNameLength);
    }

    public void SetPayment(PaymentMethod paymentMethod, ExpensePaymentStatus paymentStatus)
    {
        PaymentMethod = paymentMethod;
        PaymentStatus = paymentStatus;
    }

    public void SetAmounts(decimal amount, decimal taxAmount)
    {
        if (amount <= 0)
        {
            throw new BusinessException("NurseryHub:Expenses:AmountMustBeGreaterThanZero");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        TaxAmount = 0;
        TotalAmount = Amount;
    }

    public void SetExpenseDate(DateTime expenseDate)
    {
        ExpenseDate = expenseDate;
    }

    public void SetReceipt(string? receiptNumber, Guid? receiptFileId)
    {
        ReceiptNumber = Check.Length(receiptNumber?.Trim(), nameof(receiptNumber), MaxReceiptNumberLength);
        ReceiptFileId = receiptFileId;
    }

    public void SetNotes(string? notes)
    {
        Notes = Check.Length(notes?.Trim(), nameof(notes), MaxNotesLength);
    }
}
