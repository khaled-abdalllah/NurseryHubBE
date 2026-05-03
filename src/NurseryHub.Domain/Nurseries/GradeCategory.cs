using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NurseryHub.Nurseries;

public class GradeCategory : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;
    public const int MaxIconLength = 64;
    public const int MaxColorTokenLength = 64;
    public const int MaxDescriptionLength = 512;

    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Icon { get; private set; } = null!;
    public string ColorToken { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    protected GradeCategory()
    {
        Name = string.Empty;
        Icon = string.Empty;
        ColorToken = string.Empty;
    }

    public GradeCategory(
        Guid id,
        Guid? tenantId,
        string name,
        string icon,
        string colorToken,
        string? description = null,
        bool isActive = true) : base(id)
    {
        TenantId = tenantId;
        SetName(name);
        SetIcon(icon);
        SetColorToken(colorToken);
        SetDescription(description);
        IsActive = isActive;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), MaxNameLength);
    }

    public void SetIcon(string icon)
    {
        Icon = Check.NotNullOrWhiteSpace(icon, nameof(icon), MaxIconLength);
    }

    public void SetColorToken(string colorToken)
    {
        ColorToken = Check.NotNullOrWhiteSpace(colorToken, nameof(colorToken), MaxColorTokenLength);
    }

    public void SetDescription(string? description)
    {
        Description = (description ?? string.Empty).Trim();
        if (Description.Length > MaxDescriptionLength)
        {
            Description = Description[..MaxDescriptionLength];
        }
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
