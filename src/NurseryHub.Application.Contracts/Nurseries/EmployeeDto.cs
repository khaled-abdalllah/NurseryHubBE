using System;
using System.Collections.Generic;

namespace NurseryHub.Nurseries;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
    public List<Guid> BranchIds { get; set; } = new();
    public string BranchNames { get; set; } = string.Empty;
    public EmployeeStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}
