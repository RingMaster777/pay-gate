namespace PayGate.DTOs;

public record PaymentResponse(
    string TransactionId,
    string PaymentUrl,
    string Status);
