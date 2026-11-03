using Hangfire.Common;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace HireHub.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpiresAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "JobSeeker";

        public bool IsActive { get; set; } = true;

        public bool IsVerified { get; set; } = false;

        [MaxLength(30)]
        public string CurrentPlan { get; set; } = "Free";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public JobSeekerProfile? JobSeekerProfile { get; set; }
        public RecruiterProfile? RecruiterProfile { get; set; }
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<DailyUsage> DailyUsages { get; set; } = new List<DailyUsage>();
    }
}