using Hangfire.Dashboard;

namespace HireHub.Helpers
{
    public class HangfireAdminAuthorization : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Must be authenticated AND an Admin
            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole(RoleConstants.Admin);
        }
    }
}