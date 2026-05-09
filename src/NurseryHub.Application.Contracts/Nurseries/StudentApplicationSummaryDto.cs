namespace NurseryHub.Nurseries;

public class StudentApplicationSummaryDto
{
    public long TotalApplications { get; set; }
    public long PendingApplications { get; set; }
    public long UnderReviewApplications { get; set; }
    public long AcceptedApplications { get; set; }
    public long RejectedApplications { get; set; }
}
