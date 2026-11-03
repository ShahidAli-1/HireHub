namespace HireHub.Services.Usage
{
    public interface IUsageLimitService
    {
        /// <summary>
        /// Checks whether the user can perform the action today
        /// and increments the counter if allowed.
        /// </summary>
        /// <param name="userId">User performing the action</param>
        /// <param name="type">"Application" or "JobPost"</param>
        /// <param name="dailyLimit">Max allowed per day</param>
        Task<bool> CheckAndIncrementAsync(int userId, string type, int dailyLimit);

        /// <summary>
        /// Returns today's usage for a user.
        /// </summary>
        Task<(int ApplicationsToday, int JobsPostedToday)> GetTodayUsageAsync(int userId);
    }
}