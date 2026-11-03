using HireHub.Data;
using HireHub.Models;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Services.Usage
{
    public class UsageLimitService : IUsageLimitService
    {
        private readonly ApplicationDbContext _context;

        public UsageLimitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckAndIncrementAsync(int userId, string type, int dailyLimit)
        {
            // If limit is int.MaxValue (Pro plan), allow immediately without hitting DB
            if (dailyLimit == int.MaxValue) return true;

            var today = DateTime.UtcNow.Date;

            var usage = await _context.DailyUsages
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Date == today);

            if (usage == null)
            {
                usage = new DailyUsage
                {
                    UserId = userId,
                    Date = today,
                    ApplicationsSubmittedToday = 0,
                    JobsPostedToday = 0,
                    ResumeScoresToday = 0,
                    ResumeEditsToday = 0
                };
                _context.DailyUsages.Add(usage);
            }

            if (type == "Application")
            {
                if (usage.ApplicationsSubmittedToday >= dailyLimit) return false;
                usage.ApplicationsSubmittedToday++;
            }
            else if (type == "JobPost")
            {
                if (usage.JobsPostedToday >= dailyLimit) return false;
                usage.JobsPostedToday++;
            }
            else if (type == "ResumeScore")
            {
                if (usage.ResumeScoresToday >= dailyLimit) return false;
                usage.ResumeScoresToday++;
            }
            else if (type == "ResumeEdit")
            {
                if (usage.ResumeEditsToday >= dailyLimit) return false;
                usage.ResumeEditsToday++;
            }
            else
            {
                return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(int ApplicationsToday, int JobsPostedToday)> GetTodayUsageAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var usage = await _context.DailyUsages
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Date == today);

            if (usage == null) return (0, 0);

            return (usage.ApplicationsSubmittedToday, usage.JobsPostedToday);
        }
    }
}