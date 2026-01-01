namespace PayGate.Models;

public class WebhookLog
{
    public Guid Id { get; set; }
    public Guid? TransactionId { get; set; }
    public string Gateway { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public bool Processed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
