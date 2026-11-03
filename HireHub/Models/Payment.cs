using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(30)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Method { get; set; } = "Manual";

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? ProofImagePath { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public int? RelatedEntityId { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(ReviewedBy))]
        public User? Reviewer { get; set; }
    }
}