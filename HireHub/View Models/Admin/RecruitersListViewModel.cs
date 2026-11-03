using HireHub.Models;

namespace HireHub.ViewModels.Admin
{
    public class RecruitersListViewModel
    {
        public List<User> Recruiters { get; set; } = new();
        public int PendingCount => Recruiters.Count(u => u.RecruiterProfile?.IsVerified != true);
        public int VerifiedCount => Recruiters.Count(u => u.RecruiterProfile?.IsVerified == true);
    }
}