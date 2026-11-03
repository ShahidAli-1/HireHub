using HireHub.Models;
using HireHub.ViewModels.Jobs;

namespace HireHub.Services.Jobs
{
    public interface IJobService
    {
        // Recruiter actions
        Task<Job> CreateJobAsync(PostJobViewModel model, int recruiterId);
        Task<List<Job>> GetRecruiterJobsAsync(int recruiterId);
        Task<Job?> GetJobByIdAsync(int jobId);
        Task<List<Category>> GetActiveCategoriesAsync();
        Task<Job?> GetJobForRecruiterAsync(int jobId, int recruiterId);
        Task<bool> UpdateJobAsync(EditJobViewModel model, int recruiterId);
        Task<bool> CloseJobAsync(int jobId, int recruiterId);

        // Public actions (NEW)
        Task<JobListViewModel> GetPublicJobsAsync(JobListViewModel filter);
        Task<Job?> GetPublicJobDetailAsync(int jobId);
        Task IncrementViewCountAsync(int jobId);
        Task<bool> DeleteJobAsync(int jobId, int recruiterId);
        Task<bool> ReopenJobAsync(int jobId, int recruiterId);
    }
}