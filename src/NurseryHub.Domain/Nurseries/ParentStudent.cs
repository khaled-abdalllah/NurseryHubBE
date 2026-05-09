using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class ParentStudent : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ParentUserId { get; private set; }
    public Guid StudentId { get; private set; }

    protected ParentStudent()
    {
    }

    public ParentStudent(Guid id, Guid? tenantId, Guid parentUserId, Guid studentId) : base(id)
    {
        TenantId = tenantId;
        ParentUserId = Check.NotNull(parentUserId, nameof(parentUserId));
        StudentId = Check.NotNull(studentId, nameof(studentId));
    }
}
