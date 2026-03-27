namespace TDDTest.Application.DTOs;

public sealed record AccountDto(
    Guid Id,
    string AccountNumber,
    string OwnerName,
    string Email,
    decimal Balance,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
