using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class PaymentFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? NurseryBranchId { get; set; }
    public Guid? GradeId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentPurpose? PaymentPurpose { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Filter { get; set; }
}
