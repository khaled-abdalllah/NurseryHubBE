using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class ExpenseDto : FullAuditedEntityDto<Guid>
{
    public Guid NurseryBranchId { get; set; }
    public string ExpenseNumber { get; set; } = null!;
    public string Title { get; set; } = null!;
    public ExpenseCategory ExpenseCategory { get; set; }
    public string? CustomExpenseCategory { get; set; }
    public string? VendorName { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ExpensePaymentStatus PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Notes { get; set; }
    public Guid? ReceiptFileId { get; set; }
}
