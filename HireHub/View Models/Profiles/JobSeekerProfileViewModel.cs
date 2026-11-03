using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Profiles
{
    public class JobSeekerProfileViewModel
    {
        [Required]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public string? Skills { get; set; }
        public string? Experience { get; set; }
        public string? Education { get; set; }

        [MaxLength(100)]
        public string? ExpectedSalary { get; set; }

        public string? ExistingResumePath { get; set; }
        public IFormFile? ResumeFile { get; set; }
    }
}