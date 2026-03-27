using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TDDTest.Application.Transactions.Commands.TransferBalance;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Tests.Transactions.Commands;

public sealed class TransferBalanceCommandHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TransferBalanceCommandHandler _handler;

    public TransferBalanceCommandHandlerTests()
        => _handler = new TransferBalanceCommandHandler(_accountRepository, _unitOfWork);

    [Fact]
    public async Task Handle_ValidTransfer_ReturnsTransferResult()
    {
        var source = Account.Create("Source", "src@test.com", 1000m);
        var dest = Account.Create("Destination", "dst@test.com", 200m);
        SetupAccounts(source, dest);

        var command = new TransferBalanceCommand(source.Id, dest.Id, 300m, "Test transfer");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.SourceAccountId.Should().Be(source.Id);
        result.DestinationAccountId.Should().Be(dest.Id);
        result.Amount.Should().Be(300m);
        result.SourceBalanceAfter.Should().Be(700m);
        result.DestinationBalanceAfter.Should().Be(500m);
    }

    [Fact]
    public async Task Handle_ValidTransfer_DeductsFromSourceAndCreditsDestination()
    {
        var source = Account.Create("Source", "src@test.com", 1000m);
        var dest = Account.Create("Dest", "dst@test.com", 0m);
        SetupAccounts(source, dest);

        await _handler.Handle(
            new TransferBalanceCommand(source.Id, dest.Id, 400m, "Transfer"),
            CancellationToken.None);

        source.Balance.Should().Be(600m);
        dest.Balance.Should().Be(400m);
    }

    [Fact]
    public async Task Handle_ValidTransfer_PersistsChanges()
    {
        var source = Account.Create("A", "a@test.com", 500m);
        var dest = Account.Create("B", "b@test.com", 0m);
        SetupAccounts(source, dest);

        await _handler.Handle(
            new TransferBalanceCommand(source.Id, dest.Id, 100m, "Transfer"),
            CancellationToken.None);

        _accountRepository.Received(1).Update(source);
        _accountRepository.Received(1).Update(dest);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TransactionsLinkedByReferenceId()
    {
        var source = Account.Create("A", "a@test.com", 500m);
        var dest = Account.Create("B", "b@test.com", 0m);
        SetupAccounts(source, dest);

        var result = await _handler.Handle(
            new TransferBalanceCommand(source.Id, dest.Id, 100m, "Linked transfer"),
            CancellationToken.None);

        var destTx = dest.Transactions.Last();
        destTx.ReferenceTransactionId.Should().Be(result.SourceTransactionId);
    }

    [Fact]
    public async Task Handle_InsufficientFunds_ThrowsInsufficientFundsException()
    {
        var source = Account.Create("Poor", "poor@test.com", 50m);
        var dest = Account.Create("Rich", "rich@test.com", 0m);
        SetupAccounts(source, dest);

        var act = async () => await _handler.Handle(
            new TransferBalanceCommand(source.Id, dest.Id, 500m, "Too much"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientFundsException>();
    }

    [Fact]
    public async Task Handle_SourceAccountNotFound_ThrowsAccountNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        var dest = Account.Create("Dest", "d@test.com");
        _accountRepository.GetByIdAsync(unknownId, Arg.Any<CancellationToken>()).ReturnsNull();
        _accountRepository.GetByIdAsync(dest.Id, Arg.Any<CancellationToken>()).Returns(dest);

        var act = async () => await _handler.Handle(
            new TransferBalanceCommand(unknownId, dest.Id, 100m, "Transfer"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AccountNotFoundException>()
            .WithMessage($"*{unknownId}*");
    }

    [Fact]
    public async Task Handle_DestinationAccountNotFound_ThrowsAccountNotFoundException()
    {
        var source = Account.Create("Src", "s@test.com", 500m);
        var unknownId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(source.Id, Arg.Any<CancellationToken>()).Returns(source);
        _accountRepository.GetByIdAsync(unknownId, Arg.Any<CancellationToken>()).ReturnsNull();

        var act = async () => await _handler.Handle(
            new TransferBalanceCommand(source.Id, unknownId, 100m, "Transfer"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AccountNotFoundException>()
            .WithMessage($"*{unknownId}*");
    }

    private void SetupAccounts(Account source, Account dest)
    {
        _accountRepository.GetByIdAsync(source.Id, Arg.Any<CancellationToken>()).Returns(source);
        _accountRepository.GetByIdAsync(dest.Id, Arg.Any<CancellationToken>()).Returns(dest);
    }
}
