using HireHub.Data;
using HireHub.Helpers;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Jobs
{
    public class JobExpiryJob
    {
        private readonly ApplicationDbContext _context;

        public JobExpiryJob(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            var today = DateTime.UtcNow;

            // Find active jobs whose deadline has passed
            var expiredJobs = await _context.Jobs
                .Where(j => j.Status == StatusConstants.Active
                         && j.Deadline.HasValue
                         && j.Deadline.Value < today)
                .ToListAsync();

            if (!expiredJobs.Any()) return;

            foreach (var job in expiredJobs)
            {
                job.Status = StatusConstants.Expired;
                job.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}