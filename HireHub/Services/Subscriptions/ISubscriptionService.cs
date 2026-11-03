using HireHub.Models;

namespace HireHub.Services.Subscriptions
{
    public interface ISubscriptionService
    {
        // Subscription lifecycle
        Task<Subscription> ActivateAsync(int userId, string planType, int? paymentId = null);
        Task<List<Subscription>> GetUserSubscriptionsAsync(int userId);
        Task<Subscription?> GetActiveSubscriptionAsync(int userId);
        Task<bool> CancelAsync(int subscriptionId, int userId);

        // Payment helpers
        Task<Payment> SubmitPaymentAsync(
            int userId, decimal amount, string purpose,
            string reference, string? proofPath, string? notes);

        Task<List<Payment>> GetUserPaymentsAsync(int userId);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);

        // Admin actions (NEW)
        Task<List<Payment>> GetPendingPaymentsAsync();
        Task<(bool Success, string Error)> ApprovePaymentAsync(int paymentId, int adminId);
        Task<(bool Success, string Error)> RejectPaymentAsync(int paymentId, int adminId, string reason);
    }
}