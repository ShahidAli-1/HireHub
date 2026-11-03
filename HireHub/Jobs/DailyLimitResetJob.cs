using HireHub.Data;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Jobs
{
    public class DailyLimitResetJob
    {
        private readonly ApplicationDbContext _context;

        public DailyLimitResetJob(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            // Delete yesterday's and older usage records to keep the table clean
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            var oldUsage = await _context.DailyUsages
                .Where(d => d.Date <= yesterday)
                .ToListAsync();

            if (oldUsage.Any())
            {
                _context.DailyUsages.RemoveRange(oldUsage);
                await _context.SaveChangesAsync();
            }
        }
    }
}