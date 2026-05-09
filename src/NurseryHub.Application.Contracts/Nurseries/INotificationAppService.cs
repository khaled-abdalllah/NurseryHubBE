using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface INotificationAppService : IApplicationService
{
    Task<PagedResultDto<NotificationListDto>> GetListAsync(NotificationFilterDto input);
    Task<NotificationDetailsDto> GetAsync(Guid id);
    Task<NotificationDto> CreateAsync(CreateUpdateNotificationDto input);
    Task<NotificationDto> UpdateAsync(Guid id, CreateUpdateNotificationDto input);
    Task DeleteAsync(Guid id);
    Task SendNotificationAsync(Guid id);
    Task ScheduleNotificationAsync(Guid id, DateTime scheduledDate);
    Task<PagedResultDto<ParentSelectionDto>> GetParentsAsync(GetParentsInputDto input);
}

public class GetParentsInputDto : PagedAndSortedResultRequestDto
{
    public Guid? BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentId { get; set; }
    public string? Filter { get; set; }
}
