using HireHub.Models;

namespace HireHub.ViewModels.Jobs
{
    public class JobListViewModel
    {
        public List<Job> Jobs { get; set; } = new();

        // Filter inputs
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? JobType { get; set; }
        public string? Location { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalJobs { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalJobs / PageSize);

        // For dropdowns
        public List<Category> Categories { get; set; } = new();
        public HashSet<int> AppliedJobIds { get; set; } = new();
    }
}