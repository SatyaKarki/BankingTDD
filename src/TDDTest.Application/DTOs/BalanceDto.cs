namespace TDDTest.Application.DTOs;

public sealed record BalanceDto(
    Guid AccountId,
    string AccountNumber,
    decimal Balance,
    string Currency,
    DateTime AsOf);
