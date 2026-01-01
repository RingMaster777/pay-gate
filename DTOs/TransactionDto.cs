namespace PayGate.DTOs;

public record TransactionDto(
    string TransactionId,
    string Gateway,
    string OrderId,
    decimal Amount,
    string Status,
    DateTime? PaidAt,
    DateTime CreatedAt);
