using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class DailyFollowupActivityEntry : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid DailyFollowupBookId { get; private set; }
    public DailyFollowupActivityType ActivityType { get; private set; }

    protected DailyFollowupActivityEntry()
    {
    }

    public DailyFollowupActivityEntry(Guid id, Guid? tenantId, Guid dailyFollowupBookId, DailyFollowupActivityType activityType)
        : base(id)
    {
        TenantId = tenantId;
        DailyFollowupBookId = dailyFollowupBookId;
        ActivityType = activityType;
    }
}
