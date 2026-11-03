using HireHub.Data;
using HireHub.Helpers;
using HireHub.Models;
using HireHub.Services.Email;
using HireHub.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Services.Applications
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public ApplicationService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        // ============================================================
        // JOB SEEKER ACTIONS
        // ============================================================

        public async Task<Application> ApplyAsync(int jobId, int seekerId, string? coverLetter, string? customResumePath = null)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == jobId && j.Status == StatusConstants.Active);

            if (job == null)
                throw new InvalidOperationException("This job is no longer accepting applications.");

            var alreadyApplied = await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.JobSeekerId == seekerId);

            if (alreadyApplied)
                throw new InvalidOperationException("You have already applied for this job.");

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(p => p.UserId == seekerId);

            string? finalResumePath = !string.IsNullOrEmpty(customResumePath)
                ? customResumePath
                : profile?.ResumePath;

            var application = new Application
            {
                JobId = jobId,
                JobSeekerId = seekerId,
                CoverLetter = coverLetter,
                ResumePath = finalResumePath,
                Status = StatusConstants.Applied,
                AppliedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            job.ApplicationCount++;
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<List<Application>> GetSeekerApplicationsAsync(int seekerId)
        {
            return await _context.Applications
                .Include(a => a.Job).ThenInclude(j => j!.Category)
                .Include(a => a.Job).ThenInclude(j => j!.Recruiter).ThenInclude(r => r!.RecruiterProfile)
                .Where(a => a.JobSeekerId == seekerId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<bool> HasAppliedAsync(int jobId, int seekerId)
        {
            return await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.JobSeekerId == seekerId);
        }

        public async Task<Application?> GetByIdAsync(int applicationId, int seekerId)
        {
            return await _context.Applications
                .Include(a => a.Job).ThenInclude(j => j!.Recruiter).ThenInclude(r => r!.RecruiterProfile)
                .Include(a => a.Job).ThenInclude(j => j!.Category)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.JobSeekerId == seekerId);
        }

        // ============================================================
        // RECRUITER ACTIONS
        // ============================================================

        public async Task<List<Application>> GetApplicationsForRecruiterAsync(int recruiterId, int? jobIdFilter = null)
        {
            var query = _context.Applications
                .Include(a => a.Job).ThenInclude(j => j!.Category)
                .Include(a => a.JobSeeker).ThenInclude(u => u!.JobSeekerProfile)
                .Where(a => a.Job!.RecruiterId == recruiterId);

            if (jobIdFilter.HasValue && jobIdFilter.Value > 0)
            {
                query = query.Where(a => a.JobId == jobIdFilter.Value);
            }

            return await query
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<Application?> GetApplicationForRecruiterAsync(int applicationId, int recruiterId)
        {
            return await _context.Applications
                .Include(a => a.Job).ThenInclude(j => j!.Category)
                .Include(a => a.JobSeeker).ThenInclude(u => u!.JobSeekerProfile)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId
                                       && a.Job!.RecruiterId == recruiterId);
        }

        public async Task<bool> UpdateStatusAsync(int applicationId, string status, int recruiterId, string? notes = null, string? interviewDetails = null)
        {
            var validStatuses = new[]
            {
                StatusConstants.Applied,
                StatusConstants.Shortlisted,
                StatusConstants.Interview,
                StatusConstants.Offered,
                StatusConstants.Hired,
                StatusConstants.Rejected
            };

            if (!validStatuses.Contains(status)) return false;

            var application = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId
                                       && a.Job!.RecruiterId == recruiterId);

            if (application == null) return false;

            application.Status = status;
            application.StatusUpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(notes))
            {
                application.RecruiterNotes = notes;
            }

            if (!string.IsNullOrEmpty(interviewDetails))
            {
                application.InterviewDetails = interviewDetails;
            }

            await _context.SaveChangesAsync();

            // ==========================================
            // NEW: HIRE WORKFLOW
            // ==========================================
            if (status == StatusConstants.Hired)
            {
                // 1. Close the job
                if (application.Job != null)
                {
                    application.Job.Status = StatusConstants.Closed;
                    application.Job.UpdatedAt = DateTime.UtcNow;
                }

                // 2. Find all other applicants for this job
                var otherApplications = await _context.Applications
                    .Include(a => a.JobSeeker)
                    .Where(a => a.JobId == application.JobId && a.ApplicationId != applicationId)
                    .ToListAsync();

                // 3. Reject them and notify them
                foreach (var otherApp in otherApplications)
                {
                    if (otherApp.Status != StatusConstants.Rejected && otherApp.Status != StatusConstants.Hired)
                    {
                        otherApp.Status = StatusConstants.Rejected;
                        otherApp.StatusUpdatedAt = DateTime.UtcNow;

                        await _notificationService.CreateAsync(
                            otherApp.JobSeekerId,
                            "Application Status Updated",
                            $"The position '{application.Job?.Title}' has been filled. Thank you for applying!",
                            "Application"
                        );
                    }
                }

                await _context.SaveChangesAsync();
            }

            // ==========================================
            // SEND NOTIFICATION & EMAIL TO SEEKER
            // ==========================================
            var seekerId = application.JobSeekerId;
            var jobTitle = application.Job?.Title ?? "a job";

            await _notificationService.CreateAsync(
                seekerId,
                $"Application Update: {status}",
                $"Your application for '{jobTitle}' has been marked as {status}.",
                "Application"
            );

            if (!string.IsNullOrEmpty(application.JobSeeker?.Email))
            {
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4f46e5;'>Application Update</h2>
                        <p>Hi {application.JobSeeker.FullName},</p>
                        <p>Your application for the position of <strong>{jobTitle}</strong> has been updated.</p>
                        <div style='background-color: #f3f4f6; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <strong>New Status:</strong> <span style='color: #059669; font-size: 1.1em;'>{status}</span>
                        </div>";

                if (status == "Interview" && !string.IsNullOrEmpty(interviewDetails))
                {
                    emailBody += $@"
                        <div style='background-color: #e0e7ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <strong>Interview Details:</strong><br/>
                            {interviewDetails.Replace(" | ", "<br/>")}
                        </div>";
                }

                emailBody += @"
                        <p>Log in to your HireHub dashboard to view more details.</p>
                        <br />
                        <p>Best regards,<br/>The HireHub Team</p>
                    </div>";

                await _emailService.SendEmailAsync(
                    application.JobSeeker.Email,
                    $"Application Update: {jobTitle}",
                    emailBody
                );
            }

            return true;
        }
        public async Task<bool> WithdrawApplicationAsync(int applicationId, int seekerId)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.JobSeekerId == seekerId);

            if (application == null) return false;

            // Don't allow withdrawing if they are already hired
            if (application.Status == "Hired") return false;

            // Decrement the job's application count
            if (application.Job != null && application.Job.ApplicationCount > 0)
            {
                application.Job.ApplicationCount--;
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}