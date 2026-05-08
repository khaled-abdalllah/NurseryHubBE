using System;

namespace NurseryHub.Nurseries;

public class ExpenseListDto
{
    public Guid Id { get; set; }
    public Guid NurseryBranchId { get; set; }
    public string ExpenseNumber { get; set; } = null!;
    public string Title { get; set; } = null!;
    public ExpenseCategory ExpenseCategory { get; set; }
    public string? CustomExpenseCategory { get; set; }
    public string? VendorName { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ExpensePaymentStatus PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ExpenseDate { get; set; }
}
