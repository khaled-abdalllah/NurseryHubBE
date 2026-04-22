using System;

namespace NurseryHub.Nurseries;

public class GovernorateBranchCountDto
{
    public Guid GovernorateId { get; set; }
    public string DisplayName { get; set; } = null!;
    public int BranchCount { get; set; }
}
