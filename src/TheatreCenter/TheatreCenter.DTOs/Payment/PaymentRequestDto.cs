using System.ComponentModel.DataAnnotations;

namespace TheatreCenter.DTOs.Payment
{
    public class PaymentRequestDto
    {
        [Required]
        public int ShowId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "RUB";

        [Required]
        [StringLength(512)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(2048)]
        public string PaymentToken { get; set; } = string.Empty; //аналог CardCryptogramPacket Криптограмма платежных данных

        [StringLength(64)]
        public string? IpAddress { get; set; } 

        [StringLength(32)]
        public string? MockForceStatus { get; set; }

        public int? MockDelayMs { get; set; }
    }
}
