using FluentAssertions;
using NSubstitute;
using TDDTest.Application.Transactions.Queries.GetAccountTransactions;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Tests.Transactions.Queries;

public sealed class GetAccountTransactionsQueryHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly GetAccountTransactionsQueryHandler _handler;

    public GetAccountTransactionsQueryHandlerTests()
        => _handler = new GetAccountTransactionsQueryHandler(_accountRepository, _transactionRepository);

    [Fact]
    public async Task Handle_ExistingAccountWithTransactions_ReturnsPagedResult()
    {
        var account = Account.Create("Alice", "alice@example.com", 100m);
        account.Deposit(200m, "Second");
        account.Deposit(300m, "Third");

        _accountRepository.ExistsAsync(account.Id, Arg.Any<CancellationToken>()).Returns(true);
        _transactionRepository.GetByAccountIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account.Transactions);

        var result = await _handler.Handle(
            new GetAccountTransactionsQuery(account.Id, 1, 20),
            CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AccountNotFound_ThrowsAccountNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        _accountRepository.ExistsAsync(unknownId, Arg.Any<CancellationToken>()).Returns(false);

        var act = async () => await _handler.Handle(
            new GetAccountTransactionsQuery(unknownId), CancellationToken.None);

        await act.Should().ThrowAsync<AccountNotFoundException>();
    }

    [Fact]
    public async Task Handle_PaginationRequest_ReturnsCorrectPage()
    {
        var account = Account.Create("Bob", "bob@example.com");
        for (var i = 1; i <= 15; i++)
            account.Deposit(i * 10m, $"Deposit {i}");

        _accountRepository.ExistsAsync(account.Id, Arg.Any<CancellationToken>()).Returns(true);
        _transactionRepository.GetByAccountIdAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns(account.Transactions);

        var result = await _handler.Handle(
            new GetAccountTransactionsQuery(account.Id, PageNumber: 2, PageSize: 5),
            CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyAccount_ReturnsEmptyPagedResult()
    {
        var accountId = Guid.NewGuid();
        _accountRepository.ExistsAsync(accountId, Arg.Any<CancellationToken>()).Returns(true);
        _transactionRepository.GetByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Domain.Entities.Transaction>());

        var result = await _handler.Handle(
            new GetAccountTransactionsQuery(accountId), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
