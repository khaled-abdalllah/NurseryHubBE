using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class Payment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxPaymentNumberLength = 64;
    public const int MaxCustomPurposeLength = 128;
    public const int MaxNotesLength = 2000;

    public Guid? TenantId { get; private set; }
    public string PaymentNumber { get; private set; } = null!;
    public Guid StudentId { get; private set; }
    public Guid GradeId { get; private set; }
    public Guid ClassId { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentPurpose PaymentPurpose { get; private set; }
    public string? CustomPaymentPurpose { get; private set; }
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }
    public DateTime PaymentDate { get; private set; }

    protected Payment()
    {
    }

    public Payment(
        Guid id,
        Guid? tenantId,
        string paymentNumber,
        Guid studentId,
        Guid gradeId,
        Guid classId,
        PaymentMethod paymentMethod,
        PaymentPurpose paymentPurpose,
        string? customPaymentPurpose,
        decimal amount,
        string? notes,
        DateTime paymentDate) : base(id)
    {
        TenantId = tenantId;
        SetPaymentNumber(paymentNumber);
        SetStudentStudentAndClass(studentId, gradeId, classId);
        SetMethodAndPurpose(paymentMethod, paymentPurpose, customPaymentPurpose);
        SetAmount(amount);
        SetNotes(notes);
        SetPaymentDate(paymentDate);
    }

    public void SetStudentStudentAndClass(Guid studentId, Guid gradeId, Guid classId)
    {
        StudentId = Check.NotNull(studentId, nameof(studentId));
        GradeId = Check.NotNull(gradeId, nameof(gradeId));
        ClassId = Check.NotNull(classId, nameof(classId));
    }

    public void SetPaymentNumber(string paymentNumber)
    {
        PaymentNumber = Check.NotNullOrWhiteSpace(paymentNumber, nameof(paymentNumber), MaxPaymentNumberLength);
    }

    public void SetMethodAndPurpose(PaymentMethod paymentMethod, PaymentPurpose paymentPurpose, string? customPaymentPurpose)
    {
        PaymentMethod = paymentMethod;
        PaymentPurpose = paymentPurpose;
        var normalizedCustomPurpose = string.IsNullOrWhiteSpace(customPaymentPurpose)
            ? null
            : customPaymentPurpose.Trim();

        if (paymentPurpose == PaymentPurpose.Other && normalizedCustomPurpose.IsNullOrWhiteSpace())
        {
            throw new BusinessException("NurseryHub:Payments:CustomPurposeRequired");
        }

        CustomPaymentPurpose = Check.Length(normalizedCustomPurpose, nameof(customPaymentPurpose), MaxCustomPurposeLength);
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessException("NurseryHub:Payments:AmountMustBeGreaterThanZero");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public void SetNotes(string? notes)
    {
        Notes = Check.Length(notes?.Trim(), nameof(notes), MaxNotesLength);
    }

    public void SetPaymentDate(DateTime paymentDate)
    {
        PaymentDate = paymentDate;
    }
}
