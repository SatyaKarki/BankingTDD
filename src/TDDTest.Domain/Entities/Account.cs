using TDDTest.Domain.Enums;
using TDDTest.Domain.Exceptions;

namespace TDDTest.Domain.Entities;

/// <summary>
/// Aggregate root representing a bank account.
/// All state changes are performed through domain methods that enforce business invariants.
/// </summary>
public sealed class Account
{
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Transaction> _transactions = [];
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { } // EF Core

    /// <summary>
    /// Factory method. Creates a new active account with an optional initial deposit.
    /// </summary>
    public static Account Create(string ownerName, string email, decimal initialDeposit = 0m)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (initialDeposit < 0)
            throw new DomainValidationException("Initial deposit cannot be negative.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = GenerateAccountNumber(),
            OwnerName = ownerName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Balance = 0m,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        if (initialDeposit > 0)
            account.Deposit(initialDeposit, "Initial deposit");

        return account;
    }

    /// <summary>
    /// Deposits funds into the account and records the transaction.
    /// </summary>
    public void Deposit(decimal amount, string description)
    {
        EnsureAccountIsActive();
        EnsurePositiveAmount(amount);

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = Transaction.Create(Id, amount, Balance, TransactionType.Deposit, description);
        _transactions.Add(transaction);
    }

    /// <summary>
    /// Withdraws funds from the account if sufficient balance exists.
    /// </summary>
    public void Withdraw(decimal amount, string description)
    {
        EnsureAccountIsActive();
        EnsurePositiveAmount(amount);
        EnsureSufficientFunds(amount);

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = Transaction.Create(Id, amount, Balance, TransactionType.Withdrawal, description);
        _transactions.Add(transaction);
    }

    /// <summary>
    /// Records the debit side of an outbound transfer.
    /// </summary>
    public Transaction DebitForTransfer(decimal amount, string description)
    {
        EnsureAccountIsActive();
        EnsurePositiveAmount(amount);
        EnsureSufficientFunds(amount);

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = Transaction.Create(Id, amount, Balance, TransactionType.TransferDebit, description);
        _transactions.Add(transaction);
        return transaction;
    }

    /// <summary>
    /// Records the credit side of an inbound transfer.
    /// </summary>
    public Transaction CreditFromTransfer(decimal amount, string description, Guid? referenceTransactionId = null)
    {
        EnsureAccountIsActive();
        EnsurePositiveAmount(amount);

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = Transaction.Create(Id, amount, Balance, TransactionType.TransferCredit, description, referenceTransactionId);
        _transactions.Add(transaction);
        return transaction;
    }

    /// <summary>Freezes the account, preventing further transactions.</summary>
    public void Freeze()
    {
        if (Status == AccountStatus.Closed)
            throw new AccountOperationException("Cannot freeze a closed account.");

        Status = AccountStatus.Frozen;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Reactivates a frozen account.</summary>
    public void Unfreeze()
    {
        if (Status != AccountStatus.Frozen)
            throw new AccountOperationException("Only frozen accounts can be unfrozen.");

        Status = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Permanently closes the account.</summary>
    public void Close()
    {
        if (Status == AccountStatus.Closed)
            throw new AccountOperationException("Account is already closed.");

        if (Balance > 0)
            throw new AccountOperationException("Cannot close an account with a positive balance. Please withdraw remaining funds first.");

        Status = AccountStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private void EnsureAccountIsActive()
    {
        if (Status != AccountStatus.Active)
            throw new AccountOperationException(
                $"Account '{AccountNumber}' is {Status} and cannot perform transactions.");
    }

    private static void EnsurePositiveAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("Amount must be greater than zero.");
    }

    private void EnsureSufficientFunds(decimal amount)
    {
        if (Balance < amount)
            throw new InsufficientFundsException(amount, Balance);
    }

    private static string GenerateAccountNumber()
    {
        // Format: 10-digit zero-padded random number prefixed with "ACC-"
        var random = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L);
        return $"ACC-{random}";
    }
}
