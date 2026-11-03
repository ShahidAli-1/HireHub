using HireHub.Models;

namespace HireHub.ViewModels.Admin
{
    public class UsersListViewModel
    {
        public List<User> Users { get; set; } = new();

        // Filters
        public string? Search { get; set; }
        public string? RoleFilter { get; set; }
        public bool? ActiveFilter { get; set; }

        // Stats
        public int TotalUsers => Users.Count;
        public int ActiveCount => Users.Count(u => u.IsActive);
        public int InactiveCount => Users.Count(u => !u.IsActive);
    }
}