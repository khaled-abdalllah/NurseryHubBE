using System;

namespace NurseryHub.Nurseries;

public class NurseryUserDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreationTime { get; set; }
}
