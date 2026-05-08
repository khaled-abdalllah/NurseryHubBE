using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class ChangeEmployeeStatusDto
{
    [Required]
    public bool IsActive { get; set; }
}
