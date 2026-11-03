using HireHub.Helpers;

namespace HireHub.ViewModels.Subscriptions
{
    public class PlansViewModel
    {
        public string Role { get; set; } = string.Empty;
        public string CurrentPlan { get; set; } = "Free";
        public List<PlanDetails> Plans { get; set; } = new();
        public bool LimitReached { get; set; } = false;
    }
}