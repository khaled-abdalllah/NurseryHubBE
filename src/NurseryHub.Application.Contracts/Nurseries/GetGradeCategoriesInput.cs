using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GetGradeCategoriesInput : PagedAndSortedResultRequestDto
{
    public Guid? NurseryBranchId { get; set; }
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
