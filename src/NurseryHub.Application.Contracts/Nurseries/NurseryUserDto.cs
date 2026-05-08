using System;
using System.Collections.Generic;

namespace NurseryHub.Nurseries;

public class NurseryUserDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public List<Guid> BranchIds { get; set; } = new();
}
