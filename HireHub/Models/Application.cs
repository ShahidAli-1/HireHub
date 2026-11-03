using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public int JobSeekerId { get; set; }

        public string? CoverLetter { get; set; }

        [MaxLength(500)]
        public string? ResumePath { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Applied";

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StatusUpdatedAt { get; set; }

        [MaxLength(1000)]
        public string? RecruiterNotes { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job? Job { get; set; }

        [ForeignKey(nameof(JobSeekerId))]
        public User? JobSeeker { get; set; }
        [MaxLength(500)]
        public string? InterviewDetails { get; set; }
    }
}