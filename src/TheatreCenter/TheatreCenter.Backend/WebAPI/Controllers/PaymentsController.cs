using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs.Payment;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("charge")]
        [SwaggerOperation(Summary = "Создать платеж через CloudPayments")]
        public async Task<ActionResult<PaymentResultDto>> Charge([FromBody] PaymentRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IpAddress))
            {
                request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            }

            var result = await _paymentService.ChargeAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{transactionId}")]
        [SwaggerOperation(Summary = "Получить статус платежа")]
        public async Task<ActionResult<PaymentStatusDto>> GetStatus([FromRoute] string transactionId, CancellationToken cancellationToken)
        {
            var result = await _paymentService.GetStatusAsync(transactionId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{transactionId}/refund")]
        [SwaggerOperation(Summary = "Вернуть платеж")]
        public async Task<ActionResult<RefundResultDto>> Refund([FromRoute] string transactionId, [FromBody] RefundRequestDto request, CancellationToken cancellationToken)
        {
            if (!string.Equals(transactionId, request.TransactionId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("TransactionId mismatch");
            }

            var result = await _paymentService.RefundAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
