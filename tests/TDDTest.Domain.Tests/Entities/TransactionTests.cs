using FluentAssertions;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Enums;

namespace TDDTest.Domain.Tests.Entities;

public sealed class TransactionTests
{
    [Fact]
    public void Create_DepositTransaction_HasCorrectProperties()
    {
        var account = Account.Create("Alice", "alice@example.com");

        account.Deposit(500m, "Salary credit");

        var tx = account.Transactions.Single();
        tx.Id.Should().NotBe(Guid.Empty);
        tx.AccountId.Should().Be(account.Id);
        tx.Amount.Should().Be(500m);
        tx.BalanceAfter.Should().Be(500m);
        tx.Type.Should().Be(TransactionType.Deposit);
        tx.Description.Should().Be("Salary credit");
        tx.ReferenceTransactionId.Should().BeNull();
        tx.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithdrawalTransaction_HasCorrectProperties()
    {
        var account = Account.Create("Bob", "bob@example.com", 300m);

        account.Withdraw(100m, "Grocery store");

        var tx = account.Transactions.Last();
        tx.Type.Should().Be(TransactionType.Withdrawal);
        tx.Amount.Should().Be(100m);
        tx.BalanceAfter.Should().Be(200m);
    }

    [Fact]
    public void Create_TransferDebitTransaction_HasCorrectType()
    {
        var source = Account.Create("Carol", "carol@example.com", 1000m);

        source.DebitForTransfer(300m, "Transfer out");

        source.Transactions.Last().Type.Should().Be(TransactionType.TransferDebit);
    }

    [Fact]
    public void Create_TransferCreditTransaction_LinksReferenceId()
    {
        var source = Account.Create("Carol", "carol@example.com", 1000m);
        var dest = Account.Create("Dave", "dave@example.com");

        var debitTx = source.DebitForTransfer(300m, "Transfer");
        dest.CreditFromTransfer(300m, "Transfer in", debitTx.Id);

        dest.Transactions.Last().ReferenceTransactionId.Should().Be(debitTx.Id);
    }

    [Fact]
    public void MultipleTransactions_BalancesAreSequentiallyAccurate()
    {
        var account = Account.Create("Eve", "eve@example.com", 1000m);

        account.Deposit(500m, "Bonus");
        account.Withdraw(200m, "Rent");
        account.Deposit(100m, "Refund");

        var txList = account.Transactions.ToList();

        txList[0].BalanceAfter.Should().Be(1000m); // initial deposit
        txList[1].BalanceAfter.Should().Be(1500m); // +500
        txList[2].BalanceAfter.Should().Be(1300m); // -200
        txList[3].BalanceAfter.Should().Be(1400m); // +100
        account.Balance.Should().Be(1400m);
    }
}
