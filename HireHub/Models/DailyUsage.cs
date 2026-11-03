using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class DailyUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime Date { get; set; }

        public int JobsPostedToday { get; set; } = 0;

        public int ApplicationsSubmittedToday { get; set; } = 0;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        public int ResumeScoresToday { get; set; } = 0;
        public int ResumeEditsToday { get; set; } = 0;
    }
}