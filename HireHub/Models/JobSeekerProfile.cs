using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class JobSeekerProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(500)]
        public string? ResumePath { get; set; }

        [MaxLength(500)]
        public string? CoverLetterPath { get; set; }

        public string? Skills { get; set; }

        public string? Experience { get; set; }

        public string? Education { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? ExpectedSalary { get; set; }

        public bool IsProfileBoosted { get; set; } = false;

        public DateTime? BoostedUntil { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation: each profile belongs to one User
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}