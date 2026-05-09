using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NurseryHub.Nurseries;

public class CreateUpdateNotificationDto : IValidatableObject
{
    [Required]
    [StringLength(NotificationConsts.MaxTitleLength)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(NotificationConsts.MaxMessageLength)]
    public string Message { get; set; } = null!;

    [Required]
    public NotificationType NotificationType { get; set; }

    [Required]
    public PriorityLevel PriorityLevel { get; set; }

    [Required]
    public NotificationAudienceType AudienceType { get; set; }

    public Guid? BranchId { get; set; }
    public bool IsScheduled { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public List<Guid> SelectedParentIds { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AudienceType == NotificationAudienceType.SelectedParents && SelectedParentIds.Count == 0)
        {
            yield return new ValidationResult("Selected parents are required.", new[] { nameof(SelectedParentIds) });
        }

        if (IsScheduled && !ScheduledDate.HasValue)
        {
            yield return new ValidationResult("Scheduled date is required.", new[] { nameof(ScheduledDate) });
        }
    }
}
