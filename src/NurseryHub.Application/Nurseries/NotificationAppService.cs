using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.BranchManager}")]
public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IRepository<Notification, Guid> _notificationRepository;
    private readonly IRepository<NotificationRecipient, Guid> _recipientRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<ParentContact, Guid> _parentContactRepository;
    private readonly IRepository<NurseryClass, Guid> _classRepository;
    private readonly IRepository<NurseryBranch, Guid> _branchRepository;
    private readonly IRepository<GradeCategory, Guid> _gradeCategoryRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;
    private readonly NotificationDeliveryService _deliveryService;

    public NotificationAppService(
        IRepository<Notification, Guid> notificationRepository,
        IRepository<NotificationRecipient, Guid> recipientRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<ParentContact, Guid> parentContactRepository,
        IRepository<NurseryClass, Guid> classRepository,
        IRepository<NurseryBranch, Guid> branchRepository,
        IRepository<GradeCategory, Guid> gradeCategoryRepository,
        IRepository<UserBranch, Guid> userBranchRepository,
        NotificationDeliveryService deliveryService)
    {
        _notificationRepository = notificationRepository;
        _recipientRepository = recipientRepository;
        _studentRepository = studentRepository;
        _parentContactRepository = parentContactRepository;
        _classRepository = classRepository;
        _branchRepository = branchRepository;
        _gradeCategoryRepository = gradeCategoryRepository;
        _userBranchRepository = userBranchRepository;
        _deliveryService = deliveryService;
    }

    public async Task<PagedResultDto<NotificationListDto>> GetListAsync(NotificationFilterDto input)
    {
        var accessibleBranchIds = await GetAccessibleBranchIdsAsync(input.BranchId);
        var notifications = await _notificationRepository.GetQueryableAsync();
        var recipients = await _recipientRepository.GetQueryableAsync();

        var query = notifications
            .Where(x => accessibleBranchIds.Contains(x.BranchId))
            .WhereIf(input.NotificationType.HasValue, x => x.NotificationType == input.NotificationType)
            .WhereIf(input.PriorityLevel.HasValue, x => x.PriorityLevel == input.PriorityLevel)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.DateFrom.HasValue, x => x.SentDate >= input.DateFrom)
            .WhereIf(input.DateTo.HasValue, x => x.SentDate <= input.DateTo)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Filter!) || x.Message.Contains(input.Filter!));

        var projected = query.Select(x => new NotificationListDto
        {
            Id = x.Id,
            Title = x.Title,
            NotificationType = x.NotificationType,
            AudienceType = x.AudienceType,
            SentByUserId = x.SentByUserId,
            SentDate = x.SentDate,
            Status = x.Status,
            TotalRecipients = x.TotalRecipients,
            DeliveredCount = recipients.Count(r => r.NotificationId == x.Id && r.DeliveryStatus == NotificationDeliveryStatus.Delivered),
            FailedCount = recipients.Count(r => r.NotificationId == x.Id && r.DeliveryStatus == NotificationDeliveryStatus.Failed),
            ReadCount = recipients.Count(r => r.NotificationId == x.Id && r.DeliveryStatus == NotificationDeliveryStatus.Read),
        });

        var totalCount = await AsyncExecuter.CountAsync(projected);
        var items = await AsyncExecuter.ToListAsync(
            projected.OrderByDescending(x => x.SentDate ?? DateTime.MinValue).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<NotificationListDto>(totalCount, items);
    }

    public async Task<NotificationDetailsDto> GetAsync(Guid id)
    {
        var entity = await _notificationRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(entity.BranchId);

        var recipients = await _recipientRepository.GetListAsync(x => x.NotificationId == id);
        return new NotificationDetailsDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            NotificationType = entity.NotificationType,
            PriorityLevel = entity.PriorityLevel,
            AudienceType = entity.AudienceType,
            SentByUserId = entity.SentByUserId,
            BranchId = entity.BranchId,
            IsScheduled = entity.IsScheduled,
            ScheduledDate = entity.ScheduledDate,
            Status = entity.Status,
            TotalRecipients = entity.TotalRecipients,
            SentDate = entity.SentDate,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            DeliveredCount = recipients.Count(x => x.DeliveryStatus == NotificationDeliveryStatus.Delivered),
            FailedCount = recipients.Count(x => x.DeliveryStatus == NotificationDeliveryStatus.Failed),
            ReadCount = recipients.Count(x => x.DeliveryStatus == NotificationDeliveryStatus.Read),
            Recipients = recipients.Select(x => new NotificationRecipientDto
            {
                ParentUserId = x.ParentUserId,
                DeliveryStatus = x.DeliveryStatus,
                DeliveredAt = x.DeliveredAt,
                ReadAt = x.ReadAt,
                FailureReason = x.FailureReason,
            }).ToList()
        };
    }

    public async Task<NotificationDto> CreateAsync(CreateUpdateNotificationDto input)
    {
        var branchId = await ResolveBranchIdAsync(input.BranchId);
        var entity = new Notification(GuidGenerator.Create(), CurrentTenant.Id, input.Title, input.Message,
            input.NotificationType, input.PriorityLevel, input.AudienceType, CurrentUser.Id ?? Guid.Empty, branchId);
        entity.SetSchedule(input.IsScheduled ? input.ScheduledDate : null);
        await _notificationRepository.InsertAsync(entity, autoSave: true);
        return await MapAsync(entity);
    }

    public async Task<NotificationDto> UpdateAsync(Guid id, CreateUpdateNotificationDto input)
    {
        var entity = await _notificationRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(entity.BranchId);
        entity.SetContent(input.Title, input.Message, input.NotificationType, input.PriorityLevel, input.AudienceType);
        entity.SetSchedule(input.IsScheduled ? input.ScheduledDate : null);
        await _notificationRepository.UpdateAsync(entity, autoSave: true);
        return await MapAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _notificationRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(entity.BranchId);
        await _notificationRepository.DeleteAsync(entity);
    }

    public async Task SendNotificationAsync(Guid id)
    {
        var entity = await _notificationRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(entity.BranchId);
        entity.MarkSending();
        await _notificationRepository.UpdateAsync(entity, autoSave: true);

        var parentIds = await ResolveParentIdsAsync(entity.BranchId, entity.AudienceType, id);
        var deliveredCount = await _deliveryService.CreateInAppDeliveriesAsync(id, entity.TenantId, parentIds);
        entity.MarkSent(deliveredCount, Clock.Now);
        await _notificationRepository.UpdateAsync(entity, autoSave: true);
    }

    public async Task ScheduleNotificationAsync(Guid id, DateTime scheduledDate)
    {
        var entity = await _notificationRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(entity.BranchId);
        entity.SetSchedule(scheduledDate);
        await _notificationRepository.UpdateAsync(entity, autoSave: true);
    }

    public async Task<PagedResultDto<ParentSelectionDto>> GetParentsAsync(GetParentsInputDto input)
    {
        var accessibleBranchIds = await GetAccessibleBranchIdsAsync(input.BranchId);
        var students = await _studentRepository.GetQueryableAsync();
        var parents = await _parentContactRepository.GetQueryableAsync();
        var classes = await _classRepository.GetQueryableAsync();
        var branches = await _branchRepository.GetQueryableAsync();
        var gradeCategories = await _gradeCategoryRepository.GetQueryableAsync();

        var query = from s in students
            join p in parents on s.ParentId equals p.Id
            join b in branches on s.NurseryBranchId equals b.Id
            join c in classes on s.NurseryClassId equals c.Id into classJoin
            from c in classJoin.DefaultIfEmpty()
            join gc in gradeCategories on c.GradeCategoryId equals gc.Id into gradeJoin
            from gc in gradeJoin.DefaultIfEmpty()
            where accessibleBranchIds.Contains(s.NurseryBranchId)
            select new ParentSelectionDto
            {
                ParentId = s.Id,
                ParentName = p.FatherName,
                StudentName = s.FullName,
                ClassName = c != null ? c.Name : null,
                GradeCategoryName = gc != null ? gc.Name : null,
                BranchName = b.Name,
                BranchId = s.NurseryBranchId,
                StudentId = s.Id
            };

        query = query
            .WhereIf(input.ClassId.HasValue, x => x.ClassName != null && classes.Any(c => c.Id == input.ClassId && c.Name == x.ClassName))
            .WhereIf(input.StudentId.HasValue, x => x.StudentId == input.StudentId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.ParentName.Contains(input.Filter!) || x.StudentName.Contains(input.Filter!));

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.StudentName).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<ParentSelectionDto>(total, items);
    }

    private async Task<List<Guid>> ResolveParentIdsAsync(Guid branchId, NotificationAudienceType audienceType, Guid notificationId)
    {
        if (audienceType == NotificationAudienceType.SelectedParents)
        {
            var existing = await _recipientRepository.GetListAsync(x => x.NotificationId == notificationId);
            if (existing.Count > 0)
            {
                return existing.Select(x => x.ParentUserId).Distinct().ToList();
            }
        }

        var students = await _studentRepository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(students.Where(x => x.NurseryBranchId == branchId).Select(x => x.Id).Distinct());
    }

    private async Task<NotificationDto> MapAsync(Notification entity)
    {
        return await Task.FromResult(new NotificationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            NotificationType = entity.NotificationType,
            PriorityLevel = entity.PriorityLevel,
            AudienceType = entity.AudienceType,
            SentByUserId = entity.SentByUserId,
            BranchId = entity.BranchId,
            IsScheduled = entity.IsScheduled,
            ScheduledDate = entity.ScheduledDate,
            Status = entity.Status,
            TotalRecipients = entity.TotalRecipients,
            SentDate = entity.SentDate,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId
        });
    }

    private async Task<Guid> ResolveBranchIdAsync(Guid? inputBranchId)
    {
        if (inputBranchId.HasValue && inputBranchId.Value != Guid.Empty)
        {
            await EnsureCanAccessBranchAsync(inputBranchId.Value);
            return inputBranchId.Value;
        }

        var branchIds = await GetAccessibleBranchIdsAsync(null);
        if (branchIds.Length == 0)
        {
            throw new AbpAuthorizationException("No accessible branch found.");
        }

        return branchIds[0];
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

    private async Task<Guid[]> GetAccessibleBranchIdsAsync(Guid? requestedBranchId)
    {
        if (requestedBranchId.HasValue)
        {
            await EnsureCanAccessBranchAsync(requestedBranchId.Value);
            return new[] { requestedBranchId.Value };
        }

        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            return Array.Empty<Guid>();
        }

        var userBranches = await _userBranchRepository.GetListAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value);
        return userBranches.Select(x => x.NurseryBranchId).Distinct().ToArray();
    }
}
