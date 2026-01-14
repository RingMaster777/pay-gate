using PayGate.Services;
using PayGate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace PayGate.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<PaymentResponse>> InitiatePayment(
        [FromBody] InitiatePaymentRequest request)
    {
        var merchantId = (Guid)HttpContext.Items["MerchantId"]!;
        var result = await _paymentService.InitiatePaymentAsync(request, merchantId);
        return Ok(result);
    }

    [HttpGet("{transactionId}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(string transactionId)
    {
        var merchantId = (Guid)HttpContext.Items["MerchantId"]!;
        var transaction = await _paymentService.GetTransactionAsync(transactionId, merchantId);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }
}
