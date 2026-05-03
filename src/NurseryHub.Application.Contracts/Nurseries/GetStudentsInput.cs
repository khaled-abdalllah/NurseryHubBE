using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GetStudentsInput : PagedAndSortedResultRequestDto
{
    public Guid? NurseryBranchId { get; set; }
    public Guid? NurseryClassId { get; set; }
    public string? Filter { get; set; }
}
