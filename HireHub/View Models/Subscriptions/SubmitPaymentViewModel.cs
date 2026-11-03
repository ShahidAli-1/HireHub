using System.ComponentModel.DataAnnotations;

namespace HireHub.ViewModels.Subscriptions
{
    public class SubmitPaymentViewModel
    {
        [Required]
        public string PlanName { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Reference number is required")]
        [MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        public IFormFile? ProofImage { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}