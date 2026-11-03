using HireHub.Models;

namespace HireHub.ViewModels.Admin
{
    public class PendingPaymentsViewModel
    {
        public List<Payment> PendingPayments { get; set; } = new();
        public decimal TotalPending { get; set; }
        public int Count => PendingPayments.Count;
    }
}