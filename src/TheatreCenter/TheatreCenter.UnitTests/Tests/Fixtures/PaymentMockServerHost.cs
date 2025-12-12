using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace TheatreCenter.Tests.Fixtures
{
    /// <summary>
    /// In-process mock server that emulates CloudPayments API endpoints.
    /// </summary>
    public class PaymentMockServerHost : IAsyncDisposable
    {
        private IHost? _host;
        private readonly ConcurrentDictionary<string, PaymentState> _payments = new();
        private readonly int _port;

        public string BaseUrl => $"http://localhost:{_port}";

        private PaymentMockServerHost(int port)
        {
            _port = port;
        }

        public static async Task<PaymentMockServerHost> StartAsync(int port = 18081)
        {
            var server = new PaymentMockServerHost(port);
            await server.StartInternalAsync();
            return server;
        }

        private async Task StartInternalAsync()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                ApplicationName = typeof(PaymentMockServerHost).Assembly.FullName,
                ContentRootPath = Directory.GetCurrentDirectory(),
                WebRootPath = "wwwroot",
                EnvironmentName = Environments.Development
            });

            var app = builder.Build();

            app.MapPost("/payments/cards/charge", HandleCharge);
            app.MapPost("/payments/get", HandleGet);
            app.MapPost("/payments/refund", HandleRefund);

            _host = app;
            app.Urls.Add($"{BaseUrl}");
            await app.StartAsync();
        }

        private IResult HandleCharge(PaymentChargeRequest request)
        {
            var transactionId = Guid.NewGuid().ToString("N");
            var status = request.JsonData?.MockForceStatus?.ToLowerInvariant() switch
            {
                "declined" => "Declined",
                "pending" => "Pending",
                _ => "Completed"
            };

            if (request.JsonData?.MockDelayMs is > 0)
            {
                Thread.Sleep(request.JsonData.MockDelayMs.Value);
            }

            var state = new PaymentState
            {
                TransactionId = transactionId,
                Amount = request.Amount,
                Currency = request.Currency ?? "RUB",
                Status = status
            };

            _payments[transactionId] = state;

            var response = new CloudPaymentResponse
            {
                Success = status == "Completed" || status == "Pending",
                Message = status == "Declined" ? "Mock declined" : "OK",
                Model = state
            };

            return Results.Json(response);
        }

        private IResult HandleGet(PaymentGetRequest request)
        {
            if (request.TransactionId == null || !_payments.TryGetValue(request.TransactionId, out var state))
            {
                return Results.NotFound(new { Message = "Transaction not found" });
            }

            var response = new CloudPaymentResponse
            {
                Success = true,
                Message = "OK",
                Model = state
            };
            return Results.Json(response);
        }

        private IResult HandleRefund(PaymentRefundRequest request)
        {
            if (request.TransactionId == null || !_payments.TryGetValue(request.TransactionId, out var state))
            {
                return Results.NotFound(new { Message = "Transaction not found" });
            }

            state.Status = "Refunded";
            var response = new CloudPaymentResponse
            {
                Success = true,
                Message = "Refunded",
                Model = state
            };
            return Results.Json(response);
        }

        public async ValueTask DisposeAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }

    internal record PaymentChargeRequest(decimal Amount, string? Currency, string Description, string CardCryptogramPacket, string? IpAddress, PaymentJsonData? JsonData);
    internal record PaymentGetRequest(string TransactionId);
    internal record PaymentRefundRequest(string TransactionId, decimal Amount);

    internal class PaymentJsonData
    {
        [JsonPropertyName("mockForceStatus")]
        public string? MockForceStatus { get; set; }

        [JsonPropertyName("mockDelayMs")]
        public int? MockDelayMs { get; set; }

        [JsonPropertyName("showId")]
        public int ShowId { get; set; }
    }

    internal class CloudPaymentResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public PaymentState Model { get; set; } = new();
    }

    internal class PaymentState
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "RUB";
        public string Status { get; set; } = "Pending";
    }
}
