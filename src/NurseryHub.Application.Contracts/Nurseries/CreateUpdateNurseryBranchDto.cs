using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateNurseryBranchDto
{
    [Required]
    public Guid NurseryId { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(32)]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(512)]
    public string? AddressLine { get; set; }

    public bool IsTransportationIncluded { get; set; }

    public bool IsMealsIncluded { get; set; }

    [Required]
    public Guid GovernorateId { get; set; }

    [Required]
    public Guid CityId { get; set; }

    [StringLength(1024)]
    public string? FacebookLink { get; set; }

    public bool IsActive { get; set; } = true;
}
