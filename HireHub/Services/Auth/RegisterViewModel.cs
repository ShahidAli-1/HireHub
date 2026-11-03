using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "JobSeeker"; // JobSeeker or Recruiter

        // Optional Recruiter-only field
        [MaxLength(100)]
        public string? CompanyName { get; set; }
    }
}