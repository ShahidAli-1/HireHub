using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Jobs
{
    public class PostJobViewModel
    {
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

        [Required(ErrorMessage = "Job type is required")]
        public string JobType { get; set; } = "FullTime";

        [MaxLength(30)]
        public string? ExperienceLevel { get; set; }

        public DateTime? Deadline { get; set; }

        // Populated for the dropdown
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Categories { get; set; }
    }
}