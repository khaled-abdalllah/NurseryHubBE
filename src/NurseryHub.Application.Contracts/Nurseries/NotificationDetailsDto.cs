using System.Collections.Generic;

namespace NurseryHub.Nurseries;

public class NotificationDetailsDto : NotificationDto
{
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public int ReadCount { get; set; }
    public List<NotificationRecipientDto> Recipients { get; set; } = new();
}
