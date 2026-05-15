namespace NurseryHub.Nurseries;

public enum NotificationAudienceType
{
    AllParents = 1,
    SelectedParents = 2,
    /// <summary>Parent-initiated message to branch staff (recipients are staff user ids).</summary>
    ParentToNursery = 3,
}
