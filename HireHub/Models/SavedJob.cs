using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class SavedJob
    {
        [Key]
        public int SavedJobId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int JobId { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job? Job { get; set; }
    }
}