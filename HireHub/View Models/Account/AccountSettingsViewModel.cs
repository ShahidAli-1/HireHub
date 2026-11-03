using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Account
{
    public class AccountSettingsViewModel
    {
        [Required]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        // Password change fields (optional to fill)
        public string? CurrentPassword { get; set; }

        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string? NewPassword { get; set; }

        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string? ConfirmNewPassword { get; set; }
    }
}