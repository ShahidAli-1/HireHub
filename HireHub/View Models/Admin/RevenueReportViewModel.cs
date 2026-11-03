namespace HireHub.ViewModels.Admin
{
    public class RevenueReportViewModel
    {
        // High-level stats
        public decimal TotalRevenue { get; set; }
        public decimal MRR { get; set; }
        public int TotalTransactions { get; set; }
        public int ActiveSubscriptions { get; set; }

        // Chart data (last 6 months)
        public List<string> MonthLabels { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();

        // Plan breakdown
        public Dictionary<string, int> PlanDistribution { get; set; } = new();
        public Dictionary<string, decimal> RevenueByPlan { get; set; } = new();

        // Recent transactions
        public List<Models.Payment> RecentTransactions { get; set; } = new();
    }
}