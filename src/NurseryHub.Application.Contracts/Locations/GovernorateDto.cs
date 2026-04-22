using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Locations;

public class GovernorateDto : EntityDto<Guid>
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
}
