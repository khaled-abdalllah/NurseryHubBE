using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GetNurseryClassesInput : PagedAndSortedResultRequestDto
{
    public Guid NurseryBranchId { get; set; }

    public Guid? GradeCategoryId { get; set; }

    public string? Filter { get; set; }
}
