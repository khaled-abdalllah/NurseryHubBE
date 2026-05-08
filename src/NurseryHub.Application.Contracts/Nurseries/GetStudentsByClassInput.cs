using System;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class GetStudentsByClassInput
{
    [Required]
    public Guid GradeId { get; set; }

    [Required]
    public Guid ClassId { get; set; }

    public string? Filter { get; set; }
}
