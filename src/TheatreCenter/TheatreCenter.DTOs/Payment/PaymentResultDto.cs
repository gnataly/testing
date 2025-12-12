using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs.Payment
{
    public class PaymentResultDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "RUB";
        public string Message { get; set; } = string.Empty;
    }
}
