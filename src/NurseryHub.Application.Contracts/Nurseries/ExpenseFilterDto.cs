using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class ExpenseFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? NurseryBranchId { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }
    public string? CustomExpenseCategory { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ExpensePaymentStatus? PaymentStatus { get; set; }
    public string? VendorName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Filter { get; set; }
}
