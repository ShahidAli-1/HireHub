namespace HireHub.Helpers
{
    public class PlanDetails
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DailyLimit { get; set; }   // applications or jobs per day
        public List<string> Features { get; set; } = new();
        public bool IsPopular { get; set; } = false;
    }

    public static class PlanCatalog
    {
        public static readonly List<PlanDetails> JobSeekerPlans = new()
        {
            new PlanDetails
            {
                Name = PlanConstants.Free,
                Price = 0,
                DailyLimit = 10,
                Features = new List<string>
                {
                    "Basic profile",
                    "Upload 1 resume",
                    "Apply to 10 jobs/day",
                    "ATS Checker: 2/day",
                    "Track applications",
                    "Standard support"
                }
            },
            new PlanDetails
            {
                Name = PlanConstants.Basic,
                Price = 300,
                DailyLimit = 50,
                Features = new List<string>
                {
                    "Priority in search results",
                    "Upload up to 3 resumes",
                    "Apply to 50 jobs/day",
                    "ATS Checker: 7/day",
                    "Application tracking",
                    "Email support"
                }
            },
            new PlanDetails
            {
                Name = PlanConstants.Pro,
                Price = 700,
                DailyLimit = int.MaxValue,
                IsPopular = true,
                Features = new List<string>
                {
                    "Everything in Basic",
                    "Unlimited job applications",
                    "Unlimited ATS Checks",
                    "Early access to new jobs",
                    "Free resume review",
                    "Pro badge on profile",
                    "Priority support"
                }
            }
        };

        public static readonly List<PlanDetails> RecruiterPlans = new()
        {
            new PlanDetails
            {
                Name = PlanConstants.Free,
                Price = 0,
                DailyLimit = 2,
                Features = new List<string>
                {
                    "Post 2 jobs/day",
                    "30-day job listing",
                    "View applicants",
                    "Basic dashboard",
                    "Standard support"
                }
            },
            new PlanDetails
            {
                Name = PlanConstants.Starter,
                Price = 2000,
                DailyLimit = 10,
                Features = new List<string>
                {
                    "Post 10 jobs/day",
                    "45-day job listing",
                    "Featured job option",
                    "Applicant analytics",
                    "Email support"
                }
            },
            new PlanDetails
            {
                Name = PlanConstants.Business,
                Price = 5000,
                DailyLimit = int.MaxValue,
                IsPopular = true,
                Features = new List<string>
                {
                    "Unlimited job posts",
                    "60-day job listing",
                    "Verified recruiter badge",
                    "Bulk actions on applicants",
                    "Advanced analytics",
                    "Priority support"
                }
            },
            new PlanDetails
            {
                Name = PlanConstants.Enterprise,
                Price = 15000,
                DailyLimit = int.MaxValue,
                Features = new List<string>
                {
                    "Everything in Business",
                    "Custom API access",
                    "Dedicated account manager",
                    "Custom branding",
                    "24/7 support"
                }
            }
        };

        public static PlanDetails? GetPlan(string role, string planName)
        {
            var catalog = role == RoleConstants.JobSeeker ? JobSeekerPlans : RecruiterPlans;
            return catalog.FirstOrDefault(p => p.Name == planName);
        }
    }
}