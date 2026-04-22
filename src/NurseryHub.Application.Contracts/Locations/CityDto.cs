using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Locations;

public class CityDto : EntityDto<Guid>
{
    public Guid GovernorateId { get; set; }
    public string? GovernorateNameEn { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
}
