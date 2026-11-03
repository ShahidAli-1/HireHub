using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Profiles
{
    public class CompanyProfileViewModel
    {
        [Required]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty; // Contact person

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Industry { get; set; }

        [MaxLength(50)]
        public string? CompanySize { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(2000)]
        public string? About { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public string? ExistingLogoPath { get; set; }
        public IFormFile? LogoFile { get; set; }
    }
}