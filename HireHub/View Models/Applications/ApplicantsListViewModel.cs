using HireHub.Models;

namespace HireHub.ViewModels.Applications
{
    public class ApplicantsListViewModel
    {
        public List<Application> Applications { get; set; } = new();
        public List<Job> RecruiterJobs { get; set; } = new();  // for filter dropdown
        public int? SelectedJobId { get; set; }
        public string? SelectedStatus { get; set; } // <-- NEW

        // Counts
        public int TotalCount => Applications.Count;
        public int AppliedCount => Applications.Count(a => a.Status == "Applied");
        public int ShortlistedCount => Applications.Count(a => a.Status == "Shortlisted");
        public int InterviewCount => Applications.Count(a => a.Status == "Interview");
        public int HiredCount => Applications.Count(a => a.Status == "Hired");
        public int RejectedCount => Applications.Count(a => a.Status == "Rejected");
    }
    public class InterviewScheduleModel
    {
        public int ApplicationId { get; set; }
        public string InterviewDate { get; set; } = string.Empty;
        public string InterviewTime { get; set; } = string.Empty;
        public string MeetingLink { get; set; } = string.Empty;
    }
}