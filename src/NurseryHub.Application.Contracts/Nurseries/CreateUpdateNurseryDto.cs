using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateNurseryDto
{
    [Required]
    [StringLength(NurseryConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(NurseryConsts.MaxPhoneNumberLength)]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(NurseryConsts.MaxEmailLength)]
    public string Email { get; set; } = null!;

    [StringLength(NurseryConsts.MaxWebsiteUrlLength)]
    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
