using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var payments = new ConcurrentDictionary<string, PaymentState>();

app.MapPost("/payments/cards/charge", (PaymentChargeRequest request) =>
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
    payments[transactionId] = state;

    var response = new CloudPaymentResponse
    {
        Success = status == "Completed" || status == "Pending",
        Message = status == "Declined" ? "Mock declined" : "OK",
        Model = state
    };

    return Results.Json(response);
});

app.MapPost("/payments/get", (PaymentGetRequest request) =>
{
    if (request.TransactionId == null || !payments.TryGetValue(request.TransactionId, out var state))
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
});

app.MapPost("/payments/refund", (PaymentRefundRequest request) =>
{
    if (request.TransactionId == null || !payments.TryGetValue(request.TransactionId, out var state))
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
});

app.Run();

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
