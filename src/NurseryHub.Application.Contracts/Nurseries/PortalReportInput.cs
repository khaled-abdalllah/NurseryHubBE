using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class PortalReportInput
{
    [Required]
    public PortalReportType ReportType { get; set; }

    [Required]
    public Guid NurseryBranchId { get; set; }

    public Guid? NurseryClassId { get; set; }
    public Guid? GradeCategoryId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public ApplicationStatus? ApplicationStatus { get; set; }
    public AttendanceStatus? AttendanceStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }
    public NotificationType? NotificationType { get; set; }
    public EmployeeStatus? EmployeeStatus { get; set; }
}
