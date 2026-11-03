using HireHub.Models;

namespace HireHub.Services.Applications
{
    public interface IApplicationService
    {
        // Job seeker actions
        Task<Application> ApplyAsync(int jobId, int seekerId, string? coverLetter, string? customResumePath = null);
        Task<List<Application>> GetSeekerApplicationsAsync(int seekerId);
        Task<bool> HasAppliedAsync(int jobId, int seekerId);
        Task<Application?> GetByIdAsync(int applicationId, int seekerId);

        // Recruiter actions
        Task<List<Application>> GetApplicationsForRecruiterAsync(int recruiterId, int? jobIdFilter = null);
        Task<Application?> GetApplicationForRecruiterAsync(int applicationId, int recruiterId);
        Task<bool> UpdateStatusAsync(int applicationId, string status, int recruiterId, string? notes = null, string? interviewDetails = null);
        Task<bool> WithdrawApplicationAsync(int applicationId, int seekerId);
    }
}