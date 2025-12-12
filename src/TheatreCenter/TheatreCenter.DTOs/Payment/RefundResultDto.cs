using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs.Payment
{
    public class RefundResultDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
