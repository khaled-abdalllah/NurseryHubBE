using System;

namespace NurseryHub.Nurseries;

public class PaymentListDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = null!;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = null!;
    public Guid GradeId { get; set; }
    public string? GradeName { get; set; }
    public Guid ClassId { get; set; }
    public string? ClassName { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentPurpose PaymentPurpose { get; set; }
    public string? CustomPaymentPurpose { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
}
