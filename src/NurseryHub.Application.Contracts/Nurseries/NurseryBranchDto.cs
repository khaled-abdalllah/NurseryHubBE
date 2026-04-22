using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class NurseryBranchDto : FullAuditedEntityDto<Guid>
{
    public Guid NurseryId { get; set; }
    public string? NurseryName { get; set; }
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? AddressLine { get; set; }
    public bool IsTransportationIncluded { get; set; }
    public bool IsMealsIncluded { get; set; }
    public Guid GovernorateId { get; set; }
    public string? GovernorateNameEn { get; set; }
    public Guid CityId { get; set; }
    public string? CityNameEn { get; set; }
    public string? FacebookLink { get; set; }
    public bool IsActive { get; set; }
}
