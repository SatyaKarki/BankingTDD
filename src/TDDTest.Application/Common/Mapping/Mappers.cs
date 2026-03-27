using TDDTest.Application.DTOs;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Enums;

namespace TDDTest.Application.Common.Mapping;

internal static class AccountMapper
{
    internal static AccountDto ToDto(this Account account) => new(
        account.Id,
        account.AccountNumber,
        account.OwnerName,
        account.Email,
        account.Balance,
        account.Status.ToString(),
        account.CreatedAt,
        account.UpdatedAt);

    internal static BalanceDto ToBalanceDto(this Account account) => new(
        account.Id,
        account.AccountNumber,
        account.Balance,
        "USD",
        DateTime.UtcNow);
}

internal static class TransactionMapper
{
    internal static TransactionDto ToDto(this Transaction tx) => new(
        tx.Id,
        tx.AccountId,
        tx.Amount,
        tx.BalanceAfter,
        tx.Type.ToString(),
        tx.Description,
        tx.ReferenceTransactionId,
        tx.CreatedAt);
}
