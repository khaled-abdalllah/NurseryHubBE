using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GradeCategoryDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string ColorToken { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ClassCount { get; set; }
}
