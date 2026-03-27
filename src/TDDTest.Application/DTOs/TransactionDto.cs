namespace TDDTest.Application.DTOs;

public sealed record TransactionDto(
    Guid Id,
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter,
    string Type,
    string Description,
    Guid? ReferenceTransactionId,
    DateTime CreatedAt);
