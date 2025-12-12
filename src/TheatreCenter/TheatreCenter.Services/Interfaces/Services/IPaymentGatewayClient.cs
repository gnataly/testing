using TheatreCenter.DTOs.Payment;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IPaymentGatewayClient
    {
        Task<PaymentResultDto> ChargeAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
        Task<PaymentStatusDto> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default);
        Task<RefundResultDto> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
    }
}
