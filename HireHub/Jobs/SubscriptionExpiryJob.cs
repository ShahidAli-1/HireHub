using HireHub.Data;
using HireHub.Helpers;
using HireHub.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Jobs
{
    public class SubscriptionExpiryJob
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public SubscriptionExpiryJob(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task ExecuteAsync()
        {
            var today = DateTime.UtcNow;

            // Find subscriptions that ended before today and are still marked as Active
            var expiredSubs = await _context.Subscriptions
                .Include(s => s.User)
                .Where(s => s.Status == "Active" && s.EndDate < today)
                .ToListAsync();

            if (!expiredSubs.Any()) return;

            foreach (var sub in expiredSubs)
            {
                // Mark subscription as expired
                sub.Status = "Expired";

                // Downgrade user plan
                if (sub.User != null)
                {
                    sub.User.CurrentPlan = "Free";

                    // Send notification
                    await _notificationService.CreateAsync(
                        sub.UserId,
                        "Subscription Expired",
                        $"Your {sub.PlanType} plan has expired. You have been downgraded to the Free plan.",
                        "Subscription"
                    );
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}