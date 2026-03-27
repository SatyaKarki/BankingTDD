using TDDTest.Domain.Enums;

namespace TDDTest.Domain.Entities;

/// <summary>
/// Represents a financial transaction on an account.
/// Transactions are immutable once created.
/// </summary>
public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public TransactionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Reference to a paired transfer transaction (the other side of a transfer).
    /// </summary>
    public Guid? ReferenceTransactionId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Transaction() { } // EF Core

    internal static Transaction Create(
        Guid accountId,
        decimal amount,
        decimal balanceAfter,
        TransactionType type,
        string description,
        Guid? referenceTransactionId = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Type = type,
            Description = description,
            ReferenceTransactionId = referenceTransactionId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
