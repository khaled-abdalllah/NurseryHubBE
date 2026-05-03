using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GetNurseriesInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreationDateFrom { get; set; }

    public DateTime? CreationDateTo { get; set; }
}
