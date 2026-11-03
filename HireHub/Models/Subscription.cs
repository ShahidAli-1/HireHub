using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class Subscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(30)]
        public string PlanType { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public int? PaymentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment? Payment { get; set; }
    }
}