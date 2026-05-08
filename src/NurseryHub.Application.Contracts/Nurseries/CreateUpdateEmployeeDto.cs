using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateEmployeeDto
{
    [Required]
    [StringLength(128)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public EmployeeRole Role { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> BranchIds { get; set; } = new();

    public bool IsActive { get; set; } = true;
}
