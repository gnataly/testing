using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheatreCenter.DTOs.Payment;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Services.Options;

namespace TheatreCenter.Services.Services
{
    public class CloudPaymentsClient : IPaymentGatewayClient
    {
        private readonly HttpClient _httpClient;
        private readonly PaymentOptions _options;
        private readonly ILogger<CloudPaymentsClient> _logger;
        private bool _configured;

        public CloudPaymentsClient(HttpClient httpClient, IOptions<PaymentOptions> options, ILogger<CloudPaymentsClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        private void ConfigureClient()
        {
            if (_configured) return;

            var mode = (_options.Mode ?? "mock").ToLowerInvariant();
            var baseUrl = mode == "mock" ? _options.MockBaseUrl : _options.CloudPayments.BaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("Payment base URL is not configured");
            }

            _httpClient.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : $"{baseUrl}/");
            _httpClient.DefaultRequestHeaders.Clear();

            if (mode != "mock")
            {
                var creds = $"{_options.CloudPayments.PublicId}:{_options.CloudPayments.ApiSecret}";
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(creds));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }

            _configured = true;
        }

        public async Task<PaymentResultDto> ChargeAsync(PaymentRequestDto request, CancellationToken cancellationToken = default)
        {
            ConfigureClient();

            var payload = new
            {
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                AccountId = request.ShowId.ToString(),
                InvoiceId = request.ShowId.ToString(),
                CardCryptogramPacket = request.PaymentToken,
                IpAddress = request.IpAddress,
                JsonData = new
                {
                    showId = request.ShowId,
                    mockForceStatus = request.MockForceStatus,
                    mockDelayMs = request.MockDelayMs
                }
            };

            var response = await _httpClient.PostAsJsonAsync("payments/cards/charge", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            var cloudResponse = await response.Content.ReadFromJsonAsync<CloudPaymentResponse>(cancellationToken: cancellationToken);
            if (cloudResponse == null)
            {
                throw new InvalidOperationException("Empty response from payment provider");
            }

            return new PaymentResultDto
            {
                TransactionId = cloudResponse.Model.TransactionId,
                Status = MapStatus(cloudResponse.Model.Status),
                Amount = cloudResponse.Model.Amount,
                Currency = cloudResponse.Model.Currency ?? request.Currency,
                Message = cloudResponse.Message ?? string.Empty
            };
        }

        public async Task<PaymentStatusDto> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            ConfigureClient();
            var payload = new { TransactionId = transactionId };
            var response = await _httpClient.PostAsJsonAsync("payments/get", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            var cloudResponse = await response.Content.ReadFromJsonAsync<CloudPaymentResponse>(cancellationToken: cancellationToken);
            if (cloudResponse == null)
            {
                throw new InvalidOperationException("Empty response from payment provider");
            }

            return new PaymentStatusDto
            {
                TransactionId = transactionId,
                Status = MapStatus(cloudResponse.Model.Status),
                Message = cloudResponse.Message ?? string.Empty
            };
        }

        public async Task<RefundResultDto> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
        {
            ConfigureClient();
            var payload = new { TransactionId = transactionId, Amount = amount };
            var response = await _httpClient.PostAsJsonAsync("payments/refund", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            var cloudResponse = await response.Content.ReadFromJsonAsync<CloudPaymentResponse>(cancellationToken: cancellationToken);
            if (cloudResponse == null)
            {
                throw new InvalidOperationException("Empty response from payment provider");
            }

            return new RefundResultDto
            {
                TransactionId = transactionId,
                Status = MapStatus(cloudResponse.Model.Status),
                Message = cloudResponse.Message ?? string.Empty
            };
        }

        private PaymentStatus MapStatus(string? providerStatus)
        {
            return providerStatus?.ToLowerInvariant() switch
            {
                "completed" => PaymentStatus.Completed,
                "refunded" => PaymentStatus.Refunded,
                "declined" => PaymentStatus.Declined,
                _ => PaymentStatus.Pending
            };
        }

        private sealed class CloudPaymentResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public CloudPaymentResponseModel Model { get; set; } = new();
        }

        private sealed class CloudPaymentResponseModel
        {
            public string TransactionId { get; set; } = Guid.NewGuid().ToString("N");
            public string Status { get; set; } = "Pending";
            public decimal Amount { get; set; }
            public string Currency { get; set; } = "RUB";
        }
    }
}
