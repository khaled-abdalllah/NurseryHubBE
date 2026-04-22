using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class UserBranch : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid NurseryBranchId { get; private set; }
    public bool IsPrimary { get; private set; }

    protected UserBranch()
    {
    }

    public UserBranch(
        Guid id,
        Guid? tenantId,
        Guid userId,
        Guid nurseryBranchId,
        bool isPrimary = false) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        NurseryBranchId = nurseryBranchId;
        IsPrimary = isPrimary;
    }

    public void SetIsPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
