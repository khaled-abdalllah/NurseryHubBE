using System;

namespace NurseryHub.Nurseries;

public class ConvertStudentApplicationDto
{
    public Guid? NurseryClassId { get; set; }

    public DateOnly? EnrollmentDate { get; set; }
}
