namespace HireHub.Helpers
{
    public static class PlanLimits
    {
        // Daily application limits for job seekers
        public static readonly Dictionary<string, int> JobSeekerDailyApplications = new()
        {
            { PlanConstants.Free,  10 },
            { PlanConstants.Basic, 50 },
            { PlanConstants.Pro,   int.MaxValue }
        };

        // Daily Resume Checker limits for Job Seekers (Free: 2, Basic: 7, Pro: Unlimited)
        public static readonly Dictionary<string, int> JobSeekerDailyResumeScores = new()
        {
            { PlanConstants.Free,  2 },
            { PlanConstants.Basic, 7 },
            { PlanConstants.Pro,   int.MaxValue }
        };

        // Daily job posting limits for recruiters
        public static readonly Dictionary<string, int> RecruiterDailyJobs = new()
        {
            { PlanConstants.Free,      2 },
            { PlanConstants.Starter,   10 },
            { PlanConstants.Business,  int.MaxValue },
            { PlanConstants.Enterprise,int.MaxValue }
        };

        // --- HELPER METHODS ---

        public static int GetJobSeekerLimit(string plan)
        {
            return JobSeekerDailyApplications.TryGetValue(plan, out var limit)
                ? limit : 10;
        }

        public static int GetResumeScoreLimit(string plan)
        {
            return JobSeekerDailyResumeScores.TryGetValue(plan, out var limit)
                ? limit : 2;
        }

        public static int GetRecruiterLimit(string plan)
        {
            return RecruiterDailyJobs.TryGetValue(plan, out var limit)
                ? limit : 2;
        }
    }
}