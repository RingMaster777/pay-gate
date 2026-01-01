namespace PayGate.DTOs;

public record InitiatePaymentRequest(
    decimal Amount,
    string Currency,
    string Gateway,
    string OrderId,
    string? CustomerEmail,
    string? CustomerPhone,
    string CallbackUrl,
    string? WebhookUrl);
