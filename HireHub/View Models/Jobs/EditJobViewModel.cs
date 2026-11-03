using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Jobs
{
    public class EditJobViewModel
    {
        public int JobId { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Job description is required")]
        public string Description { get; set; } = string.Empty;

        public string? Requirements { get; set; }

        [MaxLength(100)]
        public string? SalaryRange { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [Required]
        public string JobType { get; set; } = "FullTime";

        [MaxLength(30)]
        public string? ExperienceLevel { get; set; }

        public DateTime? Deadline { get; set; }

        public string? Status { get; set; }

        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Categories { get; set; }
    }
}