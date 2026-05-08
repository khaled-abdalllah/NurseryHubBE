using System;

namespace NurseryHub.Nurseries;

public class ManagedBranchLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NurseryName { get; set; } = string.Empty;
    public string? NurseryLogoUrl { get; set; }
}
