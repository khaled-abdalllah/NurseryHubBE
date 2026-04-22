using System.Collections.Generic;

namespace NurseryHub.Nurseries;

public class NurseryStatisticsDto
{
    public int TotalNurseries { get; set; }
    public int ActiveNurseries { get; set; }
    public int InactiveNurseries { get; set; }
    public int TotalBranches { get; set; }
    public int TotalClasses { get; set; }
    public List<NurseryMonthlySeriesItemDto> NewNurseriesByMonth { get; set; } = new();
    public List<GovernorateBranchCountDto> BranchesByGovernorate { get; set; } = new();
}
