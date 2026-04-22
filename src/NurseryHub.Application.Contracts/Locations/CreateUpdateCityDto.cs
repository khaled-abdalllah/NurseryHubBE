using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Locations;

public class CreateUpdateCityDto
{
    [Required]
    public Guid GovernorateId { get; set; }

    [Required]
    [StringLength(LocationConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(LocationConsts.MaxNameLength)]
    public string NameEn { get; set; } = null!;

    [Required]
    [StringLength(LocationConsts.MaxNameLength)]
    public string NameAr { get; set; } = null!;
}
