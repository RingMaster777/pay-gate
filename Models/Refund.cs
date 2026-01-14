namespace PayGate.Models;

public class Refund
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string RefundId { get; set; } = string.Empty;
    public string? GatewayRefundId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Transaction Transaction { get; set; } = null!;
}
