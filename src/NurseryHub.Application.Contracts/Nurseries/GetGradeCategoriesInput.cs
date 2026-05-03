using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GetGradeCategoriesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
