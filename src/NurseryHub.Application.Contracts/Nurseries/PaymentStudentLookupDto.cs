using System;

namespace NurseryHub.Nurseries;

public class PaymentStudentLookupDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
}
