using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class NurseryClassDto : FullAuditedEntityDto<Guid>
{
    public Guid NurseryBranchId { get; set; }
    public Guid? GradeCategoryId { get; set; }
    public string? GradeCategoryName { get; set; }
    public string? GradeCategoryColorToken { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int? MinAgeInMonths { get; set; }
    public int? MaxAgeInMonths { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int EnrollmentPercent { get; set; }
}
