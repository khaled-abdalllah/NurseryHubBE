using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateNurseryUserDto
{
    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(64)]
    public string Role { get; set; } = null!;
}
