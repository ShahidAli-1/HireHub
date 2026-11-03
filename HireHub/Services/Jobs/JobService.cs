using HireHub.Data;
using HireHub.Helpers;
using HireHub.Models;
using HireHub.ViewModels.Jobs;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Services.Jobs
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;

        public JobService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // RECRUITER ACTIONS
        // ============================================================

        public async Task<Job> CreateJobAsync(PostJobViewModel model, int recruiterId)
        {
            var user = await _context.Users.FindAsync(recruiterId);
            int listingDays = user?.CurrentPlan switch
            {
                PlanConstants.Starter => 45,
                PlanConstants.Business => 60,
                PlanConstants.Enterprise => 60,
                _ => 30
            };

            var job = new Job
            {
                RecruiterId = recruiterId,
                CategoryId = model.CategoryId,
                Title = model.Title.Trim(),
                Description = model.Description,
                Requirements = model.Requirements,
                SalaryRange = model.SalaryRange,
                Location = model.Location,
                JobType = model.JobType,
                ExperienceLevel = model.ExperienceLevel,
                Deadline = model.Deadline ?? DateTime.UtcNow.AddDays(listingDays),
                Status = StatusConstants.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<List<Job>> GetRecruiterJobsAsync(int recruiterId)
        {
            return await _context.Jobs
                .Include(j => j.Category)
                .Where(j => j.RecruiterId == recruiterId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<Job?> GetJobByIdAsync(int jobId)
        {
            return await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Recruiter).ThenInclude(r => r!.RecruiterProfile)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Job?> GetJobForRecruiterAsync(int jobId, int recruiterId)
        {
            return await _context.Jobs
                .Include(j => j.Category)
                .FirstOrDefaultAsync(j => j.JobId == jobId && j.RecruiterId == recruiterId);
        }

        public async Task<bool> UpdateJobAsync(EditJobViewModel model, int recruiterId)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == model.JobId && j.RecruiterId == recruiterId);

            if (job == null) return false;

            job.Title = model.Title.Trim();
            job.CategoryId = model.CategoryId;
            job.Description = model.Description;
            job.Requirements = model.Requirements;
            job.SalaryRange = model.SalaryRange;
            job.Location = model.Location;
            job.JobType = model.JobType;
            job.ExperienceLevel = model.ExperienceLevel;
            job.Deadline = model.Deadline;
            job.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CloseJobAsync(int jobId, int recruiterId)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == jobId && j.RecruiterId == recruiterId);

            if (job == null) return false;

            job.Status = StatusConstants.Closed;
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // PUBLIC ACTIONS
        // ============================================================

        public async Task<JobListViewModel> GetPublicJobsAsync(JobListViewModel filter)
        {
            var query = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Recruiter).ThenInclude(r => r!.RecruiterProfile)
                .Where(j => j.Status == StatusConstants.Active)
                .AsQueryable();

            // Search by title or description
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(s));
            }

            // Filter by category
            if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            {
                query = query.Where(j => j.CategoryId == filter.CategoryId.Value);
            }

            // Filter by job type
            if (!string.IsNullOrWhiteSpace(filter.JobType))
            {
                query = query.Where(j => j.JobType == filter.JobType);
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                var loc = filter.Location.Trim().ToLower();
                query = query.Where(j => j.Location != null && j.Location.ToLower().Contains(loc));
            }

            // Count BEFORE pagination
            filter.TotalJobs = await query.CountAsync();

            // Featured jobs first, then newest
            filter.Jobs = await query
                .OrderByDescending(j => j.IsFeatured)
                .ThenByDescending(j => j.CreatedAt)
                .Skip((filter.CurrentPage - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Load categories for the filter dropdown
            filter.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return filter;
        }

        public async Task<Job?> GetPublicJobDetailAsync(int jobId)
        {
            return await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Recruiter).ThenInclude(r => r!.RecruiterProfile)
                .FirstOrDefaultAsync(j => j.JobId == jobId && j.Status == StatusConstants.Active);
        }

        public async Task IncrementViewCountAsync(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.Views++;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> DeleteJobAsync(int jobId, int recruiterId)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == jobId && j.RecruiterId == recruiterId);

            if (job == null) return false;

            // Note: Because we set up Cascade Delete in ApplicationDbContext,
            // deleting the job will automatically delete all its applications.
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReopenJobAsync(int jobId, int recruiterId)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == jobId && j.RecruiterId == recruiterId);

            if (job == null || job.Status != StatusConstants.Closed) return false;

            job.Status = StatusConstants.Active;
            job.UpdatedAt = DateTime.UtcNow;

            // Optionally, extend the deadline by 30 days from today when reopened
            job.Deadline = DateTime.UtcNow.AddDays(30);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}