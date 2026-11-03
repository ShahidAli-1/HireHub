using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace HireHub.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        public int RecruiterId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? Requirements { get; set; }

        [MaxLength(100)]
        public string? SalaryRange { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [Required]
        [MaxLength(30)]
        public string JobType { get; set; } = "FullTime";

        [MaxLength(30)]
        public string? ExperienceLevel { get; set; }

        public DateTime? Deadline { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public int Views { get; set; } = 0;

        public int ApplicationCount { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public DateTime? FeaturedUntil { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(RecruiterId))]
        public User? Recruiter { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}