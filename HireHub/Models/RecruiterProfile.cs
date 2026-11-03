using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class RecruiterProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? CompanyLogoPath { get; set; }

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

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public int? VerifiedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}