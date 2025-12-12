using TheatreCenter.DTOs.Payment;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentResultDto> ChargeAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
        Task<PaymentStatusDto> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default);
        Task<RefundResultDto> RefundAsync(RefundRequestDto request, CancellationToken cancellationToken = default);
    }
}
