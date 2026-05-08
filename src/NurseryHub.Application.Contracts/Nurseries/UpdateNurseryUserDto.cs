using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class UpdateNurseryUserDto
{
    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(64)]
    public string Role { get; set; } = null!;

    /// <summary>Required for teacher and branchManager; ignored for nurseryAdmin (all branches are assigned).</summary>
    public List<Guid>? BranchIds { get; set; }
}
