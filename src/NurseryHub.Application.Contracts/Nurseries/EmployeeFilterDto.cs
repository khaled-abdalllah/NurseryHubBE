using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class EmployeeFilterDto : PagedAndSortedResultRequestDto
{
    public EmployeeRole? Role { get; set; }
    public Guid? BranchId { get; set; }
    public EmployeeStatus? Status { get; set; }
    public string? EmployeeName { get; set; }
    public string? Email { get; set; }
}
