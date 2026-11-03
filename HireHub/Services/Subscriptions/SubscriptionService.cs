using HireHub.Data;
using HireHub.Helpers;
using HireHub.Models;
using HireHub.Services.Email;
using HireHub.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Services.Subscriptions
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public SubscriptionService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        // ============================================================
        // SUBSCRIPTION LIFECYCLE
        // ============================================================

        public async Task<Subscription> ActivateAsync(int userId, string planType, int? paymentId = null)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("User not found");

            // Deactivate existing active subscriptions
            var active = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == "Active")
                .ToListAsync();

            foreach (var old in active)
            {
                old.Status = "Expired";
            }

            // Create new subscription
            var subscription = new Subscription
            {
                UserId = userId,
                PlanType = planType,
                Role = user.Role,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active",
                PaymentId = paymentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            user.CurrentPlan = planType;

            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<List<Subscription>> GetUserSubscriptionsAsync(int userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Payment)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Subscription?> GetActiveSubscriptionAsync(int userId)
        {
            return await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CancelAsync(int subscriptionId, int userId)
        {
            var sub = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId && s.UserId == userId);

            if (sub == null || sub.Status != "Active") return false;

            sub.Status = "Cancelled";

            var user = await _context.Users.FindAsync(userId);
            if (user != null) user.CurrentPlan = "Free";

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // PAYMENTS
        // ============================================================

        public async Task<Payment> SubmitPaymentAsync(
            int userId, decimal amount, string purpose,
            string reference, string? proofPath, string? notes)
        {
            var payment = new Payment
            {
                UserId = userId,
                Amount = amount,
                Purpose = purpose,
                Method = "Manual",
                ReferenceNumber = reference,
                ProofImagePath = proofPath,
                Status = "Pending",
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        // ============================================================
        // ADMIN ACTIONS
        // ============================================================

        public async Task<List<Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.User)
                .Where(p => p.Status == "Pending")
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string Error)> ApprovePaymentAsync(int paymentId, int adminId)
        {
            var payment = await _context.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null) return (false, "Payment not found.");
            if (payment.Status != "Pending") return (false, "Payment is already processed.");

            // Extract plan name from "Subscription: Basic" → "Basic"
            string planName = "";
            if (payment.Purpose.StartsWith("Subscription:"))
            {
                planName = payment.Purpose.Replace("Subscription:", "").Trim();
            }

            if (string.IsNullOrEmpty(planName))
                return (false, "Payment does not contain a valid plan name.");

            // Validate the plan exists for that user's role
            var planDetails = PlanCatalog.GetPlan(payment.User!.Role, planName);
            if (planDetails == null)
                return (false, $"Plan '{planName}' not found for role '{payment.User.Role}'.");

            // Update payment
            payment.Status = "Approved";
            payment.ReviewedBy = adminId;
            payment.ReviewedAt = DateTime.UtcNow;

            // Activate subscription (this updates the User.CurrentPlan as well)
            await ActivateAsync(payment.UserId, planName, paymentId);

            // Save the payment changes
            await _context.SaveChangesAsync();

            // ==========================================
            // SEND NOTIFICATION & EMAIL TO USER
            // ==========================================

            // 1. In-app notification
            await _notificationService.CreateAsync(
                payment.UserId,
                "Subscription Activated",
                $"Your payment of Rs. {payment.Amount:N0} was approved. You are now on the {planName} plan!",
                "Payment"
            );

            // 2. Email notification
            if (!string.IsNullOrEmpty(payment.User?.Email))
            {
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4f46e5;'>Payment Approved!</h2>
                        <p>Hi {payment.User.FullName},</p>
                        <p>Your payment of <strong>Rs. {payment.Amount:N0}</strong> has been approved.</p>
                        <p>Your account has been successfully upgraded to the <strong>{planName}</strong> plan.</p>
                        <br />
                        <p>Thank you for choosing HireHub!</p>
                    </div>
                ";

                await _emailService.SendEmailAsync(
                    payment.User.Email,
                    "Payment Approved - HireHub",
                    emailBody
                );
            }

            return (true, "");
        }

        public async Task<(bool Success, string Error)> RejectPaymentAsync(int paymentId, int adminId, string reason)
        {
            var payment = await _context.Payments
                .Include(p => p.User) // <-- ADDED: Need this to get the user's email!
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null) return (false, "Payment not found.");
            if (payment.Status != "Pending") return (false, "Payment is already processed.");

            // Update payment status
            payment.Status = "Rejected";
            payment.ReviewedBy = adminId;
            payment.ReviewedAt = DateTime.UtcNow;
            payment.Notes = (payment.Notes ?? "") + $"\n[Rejected]: {reason}";

            await _context.SaveChangesAsync();

            // ==========================================
            // SEND NOTIFICATION & EMAIL TO USER
            // ==========================================

            // 1. In-app notification
            await _notificationService.CreateAsync(
                payment.UserId,
                "Payment Rejected",
                $"Your payment of Rs. {payment.Amount:N0} was rejected. Reason: {reason}",
                "Payment"
            );

            // 2. Email notification
            if (!string.IsNullOrEmpty(payment.User?.Email))
            {
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #dc2626;'>Payment Rejected</h2>
                        <p>Hi {payment.User.FullName},</p>
                        <p>Unfortunately, your recent payment of <strong>Rs. {payment.Amount:N0}</strong> could not be verified and was rejected.</p>
                        
                        <div style='background-color: #fee2e2; color: #b91c1c; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <strong>Reason:</strong> {reason}
                        </div>

                        <p>If you believe this is a mistake, please submit a new payment with a valid reference number, or contact our support team.</p>
                        <br />
                        <p>Best regards,<br/>The HireHub Team</p>
                    </div>
                ";

                await _emailService.SendEmailAsync(
                    payment.User.Email,
                    "Action Required: Payment Rejected - HireHub",
                    emailBody
                );
            }

            return (true, "");
        }
    }
}