using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

[Authorize(Roles =
    $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.BranchManager},{NurseryHubRoles.Teacher},{NurseryHubRoles.Accountant}")]
public class NurseryStaffInboxAppService : ApplicationService, INurseryStaffInboxAppService
{
    private readonly IRepository<Notification, Guid> _notificationRepository;
    private readonly IRepository<NotificationRecipient, Guid> _notificationRecipientRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;
    private readonly IRepository<Student, Guid> _studentRepository;

    public NurseryStaffInboxAppService(
        IRepository<Notification, Guid> notificationRepository,
        IRepository<NotificationRecipient, Guid> notificationRecipientRepository,
        IRepository<UserBranch, Guid> userBranchRepository,
        IRepository<Student, Guid> studentRepository)
    {
        _notificationRepository = notificationRepository;
        _notificationRecipientRepository = notificationRecipientRepository;
        _userBranchRepository = userBranchRepository;
        _studentRepository = studentRepository;
    }

    public async Task<List<NurseryStaffInboxNotificationDto>> GetMyInboxNotificationsAsync(int maxResultCount = 50)
    {
        var staffUserId = GetCurrentStaffUserId();
        if (maxResultCount < 1 || maxResultCount > 200)
        {
            maxResultCount = 50;
        }

        var accessibleBranchIds = await GetAccessibleBranchIdsAsync();
        if (accessibleBranchIds.Count == 0)
        {
            return new List<NurseryStaffInboxNotificationDto>();
        }

        var recipientsQ = await _notificationRecipientRepository.GetQueryableAsync();
        var notificationsQ = await _notificationRepository.GetQueryableAsync();
        var studentsQ = await _studentRepository.GetQueryableAsync();

        var query =
            from r in recipientsQ
            join n in notificationsQ on r.NotificationId equals n.Id
            join s in studentsQ on n.RelatedStudentId equals s.Id into studentJoin
            from student in studentJoin.DefaultIfEmpty()
            where r.ParentUserId == staffUserId
                && n.AudienceType == NotificationAudienceType.ParentToNursery
                && accessibleBranchIds.Contains(n.BranchId)
                && n.Status == NotificationStatus.Sent
                && r.DeliveryStatus != NotificationDeliveryStatus.Pending
            orderby n.SentDate descending, r.CreationTime descending
            select new NurseryStaffInboxNotificationDto
            {
                RecipientId = r.Id,
                NotificationId = n.Id,
                SentByUserId = n.SentByUserId,
                StudentName = student != null ? student.FullName : null,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                PriorityLevel = n.PriorityLevel,
                SentDate = n.SentDate,
                DeliveryStatus = r.DeliveryStatus,
                ReadAt = r.ReadAt,
            };

        return await AsyncExecuter.ToListAsync(query.Take(maxResultCount));
    }

    public async Task<int> GetMyUnreadInboxNotificationCountAsync()
    {
        var staffUserId = GetCurrentStaffUserId();
        var accessibleBranchIds = await GetAccessibleBranchIdsAsync();
        if (accessibleBranchIds.Count == 0)
        {
            return 0;
        }

        var recipientsQ = await _notificationRecipientRepository.GetQueryableAsync();
        var notificationsQ = await _notificationRepository.GetQueryableAsync();

        var query =
            from r in recipientsQ
            join n in notificationsQ on r.NotificationId equals n.Id
            where r.ParentUserId == staffUserId
                && n.AudienceType == NotificationAudienceType.ParentToNursery
                && accessibleBranchIds.Contains(n.BranchId)
                && n.Status == NotificationStatus.Sent
                && r.DeliveryStatus != NotificationDeliveryStatus.Pending
                && r.ReadAt == null
                && r.DeliveryStatus != NotificationDeliveryStatus.Read
            select r.Id;

        return await AsyncExecuter.CountAsync(query);
    }

    [RemoteService(IsEnabled = false)]
    public async Task MarkMyInboxNotificationReadAsync(Guid recipientId)
    {
        var staffUserId = GetCurrentStaffUserId();
        var recipient = await _notificationRecipientRepository.GetAsync(recipientId);
        if (recipient.ParentUserId != staffUserId)
        {
            throw new AbpAuthorizationException("Notification does not belong to the current user.");
        }

        var notification = await _notificationRepository.GetAsync(recipient.NotificationId);
        if (notification.AudienceType != NotificationAudienceType.ParentToNursery)
        {
            throw new AbpAuthorizationException("Notification is not a parent inbox message.");
        }

        await EnsureCanAccessBranchAsync(notification.BranchId);

        if (recipient.DeliveryStatus == NotificationDeliveryStatus.Read)
        {
            return;
        }

        recipient.MarkRead(Clock.Now);
        await _notificationRecipientRepository.UpdateAsync(recipient);
    }

    private Guid GetCurrentStaffUserId()
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not authenticated.");
        }

        return CurrentUser.Id.Value;
    }

    private async Task<List<Guid>> GetAccessibleBranchIdsAsync()
    {
        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            return new List<Guid>();
        }

        var userBranches = await _userBranchRepository.GetListAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value);

        return userBranches.Select(x => x.NurseryBranchId).Distinct().ToList();
    }

    private async Task EnsureCanAccessBranchAsync(Guid branchId)
    {
        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }

        var canAccess = await _userBranchRepository.AnyAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value &&
            x.NurseryBranchId == branchId);
        if (!canAccess)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }
    }
}
