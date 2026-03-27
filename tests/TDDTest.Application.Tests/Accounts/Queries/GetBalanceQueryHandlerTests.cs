using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TDDTest.Application.Accounts.Queries.GetBalance;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Tests.Accounts.Queries;

public sealed class GetBalanceQueryHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly GetBalanceQueryHandler _handler;

    public GetBalanceQueryHandlerTests()
        => _handler = new GetBalanceQueryHandler(_accountRepository);

    [Fact]
    public async Task Handle_ExistingAccount_ReturnsCorrectBalance()
    {
        var account = Account.Create("Bob", "bob@example.com", 750m);
        _accountRepository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        var result = await _handler.Handle(new GetBalanceQuery(account.Id), CancellationToken.None);

        result.AccountId.Should().Be(account.Id);
        result.Balance.Should().Be(750m);
        result.Currency.Should().Be("USD");
        result.AccountNumber.Should().Be(account.AccountNumber);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsAccountNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(unknownId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = async () => await _handler.Handle(new GetBalanceQuery(unknownId), CancellationToken.None);

        await act.Should().ThrowAsync<AccountNotFoundException>()
            .WithMessage($"*{unknownId}*");
    }

    [Fact]
    public async Task Handle_AccountWithDeposits_ReturnsAccumulatedBalance()
    {
        var account = Account.Create("Carol", "carol@example.com", 200m);
        account.Deposit(300m, "Second deposit");
        _accountRepository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        var result = await _handler.Handle(new GetBalanceQuery(account.Id), CancellationToken.None);

        result.Balance.Should().Be(500m);
    }
}
