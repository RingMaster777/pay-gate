using PayGate.Data;
using PayGate.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace PayGate.Services;

public class WebhookService
{
    private readonly AppDbContext _context;
    private readonly BkashService _bkash;
    private readonly StripeService _stripe;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        AppDbContext context,
        BkashService bkash,
        StripeService stripe,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookService> logger)
    {
        _context = context;
        _bkash = bkash;
        _stripe = stripe;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ProcessBkashWebhookAsync(Dictionary<string, string> payload)
    {
        var paymentId = payload.GetValueOrDefault("paymentID");
        if (string.IsNullOrEmpty(paymentId))
            return;

        // Log webhook
        var log = new WebhookLog
        {
            Gateway = "bkash",
            Payload = JsonSerializer.Serialize(payload),
            Processed = false
        };
        _context.WebhookLogs.Add(log);

        // Find transaction
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.GatewayTransactionId == paymentId);

        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for bKash payment: {PaymentId}", paymentId);
            await _context.SaveChangesAsync();
            return;
        }

        log.TransactionId = transaction.Id;

        // Verify payment
        var isSuccess = await _bkash.VerifyPaymentAsync(paymentId);

        if (isSuccess)
        {
            transaction.Status = "success";
            transaction.PaidAt = DateTime.UtcNow;
            _logger.LogInformation("Payment successful: {TxnId}", transaction.TransactionId);
        }
        else
        {
            transaction.Status = "failed";
        }

        log.Processed = true;
        await _context.SaveChangesAsync();

        // Forward to merchant webhook
        if (!string.IsNullOrEmpty(transaction.WebhookUrl))
        {
            await ForwardToMerchantAsync(transaction);
        }
    }

    public async Task ProcessStripeWebhookAsync(Dictionary<string, string> payload)
    {
        var paymentIntentId = payload.GetValueOrDefault("id");
        if (string.IsNullOrEmpty(paymentIntentId))
            return;

        // Log webhook
        var log = new WebhookLog
        {
            Gateway = "stripe",
            Payload = JsonSerializer.Serialize(payload),
            Processed = false
        };
        _context.WebhookLogs.Add(log);

        // Find transaction
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.GatewayTransactionId == paymentIntentId);

        if (transaction == null)
        {
            await _context.SaveChangesAsync();
            return;
        }

        log.TransactionId = transaction.Id;

        // Verify payment
        var isSuccess = await _stripe.VerifyPaymentAsync(paymentIntentId);

        if (isSuccess)
        {
            transaction.Status = "success";
            transaction.PaidAt = DateTime.UtcNow;
        }
        else
        {
            transaction.Status = "failed";
        }

        log.Processed = true;
        await _context.SaveChangesAsync();

        // Forward to merchant webhook
        if (!string.IsNullOrEmpty(transaction.WebhookUrl))
        {
            await ForwardToMerchantAsync(transaction);
        }
    }

    private async Task ForwardToMerchantAsync(Transaction transaction)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            var webhookData = new
            {
                transactionId = transaction.TransactionId,
                orderId = transaction.OrderId,
                status = transaction.Status,
                amount = transaction.Amount,
                currency = transaction.Currency,
                paidAt = transaction.PaidAt
            };

            var content = new StringContent(
                JsonSerializer.Serialize(webhookData),
                Encoding.UTF8,
                "application/json");

            await client.PostAsync(transaction.WebhookUrl, content);

            _logger.LogInformation("Forwarded webhook to merchant: {Url}", transaction.WebhookUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forward webhook to merchant");
        }
    }
}
