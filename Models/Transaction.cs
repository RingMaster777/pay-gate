namespace PayGate.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // pending, success, failed
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? PaymentUrl { get; set; }
    public string? CallbackUrl { get; set; }
    public string? WebhookUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
