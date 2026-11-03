using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Admin
{
    public class CreateAdminViewModel
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

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }
}