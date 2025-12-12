using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheatreCenter.DTOs.Payment;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Services.Options;

namespace TheatreCenter.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentGatewayClient _gatewayClient;
        private readonly IShowRepository _showRepository;
        private readonly PaymentOptions _options;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentGatewayClient gatewayClient,
            IShowRepository showRepository,
            IOptions<PaymentOptions> options,
            ILogger<PaymentService> logger)
        {
            _gatewayClient = gatewayClient ?? throw new ArgumentNullException(nameof(gatewayClient));
            _showRepository = showRepository ?? throw new ArgumentNullException(nameof(showRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        }

        public async Task<PaymentResultDto> ChargeAsync(PaymentRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var show = await _showRepository.GetByIdAsync(request.ShowId);
            if (show == null)
            {
                throw new InvalidOperationException($"Show {request.ShowId} not found");
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                request.Currency = _options.DefaultCurrency;
            }

            _logger.LogInformation("Initiating payment for show {ShowId} amount {Amount} {Currency}", request.ShowId, request.Amount, request.Currency);
            var result = await _gatewayClient.ChargeAsync(request, cancellationToken);
            _logger.LogInformation("Payment result for show {ShowId}: {Status} tx={TransactionId}", request.ShowId, result.Status, result.TransactionId);
            return result;
        }

        public Task<PaymentStatusDto> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentNullException(nameof(transactionId));
            return _gatewayClient.GetStatusAsync(transactionId, cancellationToken);
        }

        public Task<RefundResultDto> RefundAsync(RefundRequestDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            if (string.IsNullOrWhiteSpace(request.TransactionId)) throw new ArgumentNullException(nameof(request.TransactionId));
            if (request.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(request.Amount));

            _logger.LogInformation("Refund requested for tx={TransactionId} amount={Amount}", request.TransactionId, request.Amount);
            return _gatewayClient.RefundAsync(request.TransactionId, request.Amount, cancellationToken);
        }
    }
}
