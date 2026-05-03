using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class NurseryClass : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;

    public Guid? TenantId { get; private set; }
    public Guid NurseryBranchId { get; private set; }
    public Guid? GradeCategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public int Capacity { get; private set; }
    public int? MinAgeInMonths { get; private set; }
    public int? MaxAgeInMonths { get; private set; }
    public bool IsActive { get; private set; }

    protected NurseryClass()
    {
        Name = string.Empty;
    }

    public NurseryClass(
        Guid id,
        Guid? tenantId,
        Guid nurseryBranchId,
        Guid? gradeCategoryId,
        string name,
        int capacity,
        int? minAgeInMonths = null,
        int? maxAgeInMonths = null,
        bool isActive = true) : base(id)
    {
        TenantId = tenantId;
        NurseryBranchId = nurseryBranchId;
        GradeCategoryId = gradeCategoryId;
        SetName(name);
        SetCapacity(capacity);
        SetAgeRange(minAgeInMonths, maxAgeInMonths);
        IsActive = isActive;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), MaxNameLength);
    }

    public void SetGradeCategory(Guid? gradeCategoryId)
    {
        GradeCategoryId = gradeCategoryId;
    }

    public void SetCapacity(int capacity)
    {
        Capacity = Check.Range(capacity, nameof(capacity), 1, 500);
    }

    public void SetAgeRange(int? minAgeInMonths, int? maxAgeInMonths)
    {
        if (minAgeInMonths.HasValue)
        {
            Check.Range(minAgeInMonths.Value, nameof(minAgeInMonths), 0, 144);
        }

        if (maxAgeInMonths.HasValue)
        {
            Check.Range(maxAgeInMonths.Value, nameof(maxAgeInMonths), 0, 144);
        }

        if (minAgeInMonths.HasValue && maxAgeInMonths.HasValue && minAgeInMonths > maxAgeInMonths)
        {
            throw new ArgumentException("Minimum age cannot be greater than maximum age.");
        }

        MinAgeInMonths = minAgeInMonths;
        MaxAgeInMonths = maxAgeInMonths;
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
