namespace NurseryHub.Nurseries;

public enum NotificationType
{
    GeneralAnnouncement = 1,
    PaymentReminder = 2,
    AttendanceAlert = 3,
    EventReminder = 4,
    EmergencyNotice = 5,
    ActivityUpdate = 6,
    HomeworkReminder = 7,
    /// <summary>Message sent by a parent to the nursery branch inbox.</summary>
    ParentMessage = 8,
}
