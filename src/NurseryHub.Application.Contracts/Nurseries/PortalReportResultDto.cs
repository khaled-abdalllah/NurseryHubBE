using System;
using System.Collections.Generic;

namespace NurseryHub.Nurseries;

public class PortalReportResultDto
{
    public PortalReportType ReportType { get; set; }
    public string TitleKey { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public List<PortalReportCriteriaItemDto> Criteria { get; set; } = new();
    public List<PortalReportColumnDto> Columns { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
    public int TotalRows { get; set; }
}

public class PortalReportCriteriaItemDto
{
    public string LabelKey { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public class PortalReportColumnDto
{
    public string Key { get; set; } = null!;
    public string LabelKey { get; set; } = null!;
}
