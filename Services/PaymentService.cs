using PayGate.Data;
using PayGate.Models;
using PayGate.DTOs;
using Microsoft.EntityFrameworkCore;

namespace PayGate.Services;

public class PaymentService
{
    private readonly AppDbContext _context;
    private readonly BkashService _bkash;
    private readonly StripeService _stripe;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext context,
        BkashService bkash,
        StripeService stripe,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _bkash = bkash;
        _stripe = stripe;
        _logger = logger;
    }

    public async Task<PaymentResponse> InitiatePaymentAsync(
        InitiatePaymentRequest request,
        Guid merchantId)
    {
        // Create transaction record
        var transaction = new Transaction
        {
            MerchantId = merchantId,
            TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..30],
            Gateway = request.Gateway.ToLower(),
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = "pending",
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            CallbackUrl = request.CallbackUrl,
            WebhookUrl = request.WebhookUrl
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Call appropriate gateway
        string? paymentUrl = null;
        string? gatewayTxnId = null;

        try
        {
            if (request.Gateway.ToLower() == "bkash")
            {
                var result = await _bkash.CreatePaymentAsync(
                    transaction.TransactionId,
                    request.Amount,
                    request.OrderId,
                    request.CallbackUrl);

                paymentUrl = result.PaymentUrl;
                gatewayTxnId = result.PaymentId;
            }
            else if (request.Gateway.ToLower() == "stripe")
            {
                var result = await _stripe.CreatePaymentAsync(
                    transaction.TransactionId,
                    request.Amount,
                    request.Currency,
                    request.CallbackUrl);

                paymentUrl = result.PaymentUrl;
                gatewayTxnId = result.PaymentIntentId;
            }
            else
            {
                throw new Exception("Unsupported gateway");
            }

            // Update transaction
            transaction.PaymentUrl = paymentUrl;
            transaction.GatewayTransactionId = gatewayTxnId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment initiated: {TxnId} via {Gateway}", 
                transaction.TransactionId, request.Gateway);

            return new PaymentResponse(
                transaction.TransactionId,
                paymentUrl!,
                "pending");
        }
        catch (Exception ex)
        {
            transaction.Status = "failed";
            transaction.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            throw;
        }
    }

    public async Task<TransactionDto?> GetTransactionAsync(string transactionId, Guid merchantId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => 
                t.TransactionId == transactionId && 
                t.MerchantId == merchantId);

        if (transaction == null)
            return null;

        return new TransactionDto(
            transaction.TransactionId,
            transaction.Gateway,
            transaction.OrderId,
            transaction.Amount,
            transaction.Status,
            transaction.PaidAt,
            transaction.CreatedAt);
    }
}
