using System;
using Volo.Abp.Application.Dtos;

namespace NurseryHub.Nurseries;

public class NotificationFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? BranchId { get; set; }
    public NotificationType? NotificationType { get; set; }
    public PriorityLevel? PriorityLevel { get; set; }
    public NotificationStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Filter { get; set; }
}
