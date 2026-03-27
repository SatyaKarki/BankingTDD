using FluentAssertions;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Enums;
using TDDTest.Domain.Exceptions;

namespace TDDTest.Domain.Tests.Entities;

public sealed class AccountTests
{
    // ── Account.Create ───────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidInputs_ReturnsActiveAccount()
    {
        var account = Account.Create("Jane Doe", "jane@example.com", 500m);

        account.OwnerName.Should().Be("Jane Doe");
        account.Email.Should().Be("jane@example.com");
        account.Balance.Should().Be(500m);
        account.Status.Should().Be(AccountStatus.Active);
        account.AccountNumber.Should().StartWith("ACC-");
        account.Id.Should().NotBe(Guid.Empty);
        account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithZeroInitialDeposit_ReturnsAccountWithZeroBalance()
    {
        var account = Account.Create("John Smith", "john@example.com", 0m);

        account.Balance.Should().Be(0m);
        account.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithPositiveInitialDeposit_RecordsDepositTransaction()
    {
        var account = Account.Create("Alice", "alice@example.com", 1000m);

        account.Transactions.Should().HaveCount(1);
        account.Transactions[0].Type.Should().Be(TransactionType.Deposit);
        account.Transactions[0].Amount.Should().Be(1000m);
        account.Transactions[0].BalanceAfter.Should().Be(1000m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankOwnerName_ThrowsArgumentException(string? ownerName)
    {
        var act = () => Account.Create(ownerName!, "valid@email.com");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankEmail_ThrowsArgumentException(string? email)
    {
        var act = () => Account.Create("Valid Name", email!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeInitialDeposit_ThrowsDomainValidationException()
    {
        var act = () => Account.Create("Bob", "bob@example.com", -100m);
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Create_EmailIsNormalisedToLowercase()
    {
        var account = Account.Create("Alice", "ALICE@EXAMPLE.COM");
        account.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void Create_TwoAccounts_HaveDifferentAccountNumbers()
    {
        var a1 = Account.Create("A", "a@a.com");
        var a2 = Account.Create("B", "b@b.com");

        a1.AccountNumber.Should().NotBe(a2.AccountNumber);
    }

    // ── Deposit ──────────────────────────────────────────────────────────────

    [Fact]
    public void Deposit_ValidAmount_IncreasesBalance()
    {
        var account = Account.Create("Alice", "alice@example.com", 100m);

        account.Deposit(250m, "Salary");

        account.Balance.Should().Be(350m);
    }

    [Fact]
    public void Deposit_ValidAmount_RecordsTransaction()
    {
        var account = Account.Create("Alice", "alice@example.com");

        account.Deposit(500m, "Bonus");

        account.Transactions.Should().HaveCount(1);
        account.Transactions[0].Amount.Should().Be(500m);
        account.Transactions[0].Type.Should().Be(TransactionType.Deposit);
        account.Transactions[0].BalanceAfter.Should().Be(500m);
        account.Transactions[0].Description.Should().Be("Bonus");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Deposit_NonPositiveAmount_ThrowsDomainValidationException(decimal amount)
    {
        var account = Account.Create("Alice", "alice@example.com");
        var act = () => account.Deposit(amount, "Bad deposit");
        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Deposit_OnFrozenAccount_ThrowsAccountOperationException()
    {
        var account = Account.Create("Alice", "alice@example.com");
        account.Freeze();

        var act = () => account.Deposit(100m, "Attempted deposit");
        act.Should().Throw<AccountOperationException>()
            .WithMessage("*Frozen*");
    }

    [Fact]
    public void Deposit_OnClosedAccount_ThrowsAccountOperationException()
    {
        var account = Account.Create("Alice", "alice@example.com", 0m);
        account.Close();

        var act = () => account.Deposit(100m, "Attempted deposit");
        act.Should().Throw<AccountOperationException>()
            .WithMessage("*Closed*");
    }

    // ── Withdraw ─────────────────────────────────────────────────────────────

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        var account = Account.Create("Bob", "bob@example.com", 500m);

        account.Withdraw(200m, "ATM withdrawal");

        account.Balance.Should().Be(300m);
    }

    [Fact]
    public void Withdraw_ValidAmount_RecordsWithdrawalTransaction()
    {
        var account = Account.Create("Bob", "bob@example.com", 500m);

        account.Withdraw(200m, "ATM withdrawal");

        var withdrawal = account.Transactions.Last();
        withdrawal.Type.Should().Be(TransactionType.Withdrawal);
        withdrawal.Amount.Should().Be(200m);
        withdrawal.BalanceAfter.Should().Be(300m);
    }

    [Fact]
    public void Withdraw_MoreThanBalance_ThrowsInsufficientFundsException()
    {
        var account = Account.Create("Bob", "bob@example.com", 100m);

        var act = () => account.Withdraw(150m, "Overspend");
        act.Should().Throw<InsufficientFundsException>()
            .Which.RequestedAmount.Should().Be(150m);
    }

    [Fact]
    public void Withdraw_ExactBalance_ReducesBalanceToZero()
    {
        var account = Account.Create("Bob", "bob@example.com", 100m);

        account.Withdraw(100m, "Full withdrawal");

        account.Balance.Should().Be(0m);
    }

    // ── Transfer ─────────────────────────────────────────────────────────────

    [Fact]
    public void DebitForTransfer_ValidAmount_DecreasesBalance()
    {
        var account = Account.Create("Carol", "carol@example.com", 1000m);

        account.DebitForTransfer(400m, "Transfer to savings");

        account.Balance.Should().Be(600m);
        account.Transactions.Last().Type.Should().Be(TransactionType.TransferDebit);
    }

    [Fact]
    public void CreditFromTransfer_ValidAmount_IncreasesBalance()
    {
        var account = Account.Create("Dave", "dave@example.com", 50m);

        account.CreditFromTransfer(200m, "Transfer from checking");

        account.Balance.Should().Be(250m);
        account.Transactions.Last().Type.Should().Be(TransactionType.TransferCredit);
    }

    // ── Freeze / Unfreeze / Close ────────────────────────────────────────────

    [Fact]
    public void Freeze_ActiveAccount_SetsStatusToFrozen()
    {
        var account = Account.Create("Eve", "eve@example.com");

        account.Freeze();

        account.Status.Should().Be(AccountStatus.Frozen);
    }

    [Fact]
    public void Unfreeze_FrozenAccount_SetsStatusToActive()
    {
        var account = Account.Create("Eve", "eve@example.com");
        account.Freeze();

        account.Unfreeze();

        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void Unfreeze_ActiveAccount_ThrowsAccountOperationException()
    {
        var account = Account.Create("Eve", "eve@example.com");
        var act = () => account.Unfreeze();
        act.Should().Throw<AccountOperationException>();
    }

    [Fact]
    public void Freeze_ClosedAccount_ThrowsAccountOperationException()
    {
        var account = Account.Create("Eve", "eve@example.com", 0m);
        account.Close();

        var act = () => account.Freeze();
        act.Should().Throw<AccountOperationException>();
    }

    [Fact]
    public void Close_AccountWithZeroBalance_SetsStatusToClosed()
    {
        var account = Account.Create("Frank", "frank@example.com", 0m);

        account.Close();

        account.Status.Should().Be(AccountStatus.Closed);
    }

    [Fact]
    public void Close_AccountWithPositiveBalance_ThrowsAccountOperationException()
    {
        var account = Account.Create("Frank", "frank@example.com", 100m);

        var act = () => account.Close();
        act.Should().Throw<AccountOperationException>()
            .WithMessage("*positive balance*");
    }

    [Fact]
    public void Close_AlreadyClosedAccount_ThrowsAccountOperationException()
    {
        var account = Account.Create("Frank", "frank@example.com", 0m);
        account.Close();

        var act = () => account.Close();
        act.Should().Throw<AccountOperationException>();
    }

    // ── UpdatedAt tracking ───────────────────────────────────────────────────

    [Fact]
    public void Deposit_SetsUpdatedAt()
    {
        var account = Account.Create("Grace", "grace@example.com");
        account.UpdatedAt.Should().BeNull();

        account.Deposit(10m, "Test");

        account.UpdatedAt.Should().NotBeNull()
            .And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
