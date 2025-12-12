using System.ComponentModel.DataAnnotations;

namespace TheatreCenter.DTOs.Payment
{
    public class RefundRequestDto
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
