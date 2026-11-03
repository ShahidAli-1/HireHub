using System.ComponentModel.DataAnnotations;
using HireHub.Models;

namespace HireHub.ViewModels.Applications
{
    public class ApplyJobViewModel
    {
        public int JobId { get; set; }
        public Job? Job { get; set; }

        // Existing profile resume
        public string? ProfileResumePath { get; set; }
        public bool HasProfileResume { get; set; }

        // Option to upload a new one
        public IFormFile? NewResumeFile { get; set; }
        public bool UseNewResume { get; set; } = false;

        [Required(ErrorMessage = "Please write a short cover letter")]
        [MaxLength(5000)]
        public string CoverLetter { get; set; } = string.Empty;
    }
}