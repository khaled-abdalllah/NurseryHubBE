using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class GetNurseryBranchesInput : PagedAndSortedResultRequestDto
{
    public Guid? NurseryId { get; set; }
}
