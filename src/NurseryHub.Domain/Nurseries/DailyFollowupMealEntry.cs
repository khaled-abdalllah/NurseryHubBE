using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class DailyFollowupMealEntry : Entity<Guid>, IMultiTenant
{
    public const int MaxNotesLength = 500;

    public Guid? TenantId { get; private set; }
    public Guid DailyFollowupBookId { get; private set; }
    public DailyFollowupMealType MealType { get; private set; }
    public DailyFollowupMealStatus Status { get; private set; }
    public string? Notes { get; private set; }

    protected DailyFollowupMealEntry()
    {
    }

    public DailyFollowupMealEntry(
        Guid id,
        Guid? tenantId,
        Guid dailyFollowupBookId,
        DailyFollowupMealType mealType,
        DailyFollowupMealStatus status,
        string? notes) : base(id)
    {
        TenantId = tenantId;
        DailyFollowupBookId = dailyFollowupBookId;
        MealType = mealType;
        Status = status;
        Notes = Check.Length(notes, nameof(notes), MaxNotesLength);
    }
}
