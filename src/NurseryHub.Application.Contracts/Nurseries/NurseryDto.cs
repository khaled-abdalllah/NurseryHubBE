using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class NurseryDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string? NurseryCode { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; }
    public int BranchCount { get; set; }
    public int UserCount { get; set; }
}
