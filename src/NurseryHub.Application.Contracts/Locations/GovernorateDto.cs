using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Locations;

public class GovernorateDto : CreationAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
}
