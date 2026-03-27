using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TDDTest.Application.Transactions.Queries.GetTransaction;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Tests.Transactions.Queries;

public sealed class GetTransactionQueryHandlerTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly GetTransactionQueryHandler _handler;

    public GetTransactionQueryHandlerTests()
        => _handler = new GetTransactionQueryHandler(_transactionRepository);

    [Fact]
    public async Task Handle_ExistingTransaction_ReturnsTransactionDto()
    {
        var account = Account.Create("Alice", "alice@example.com", 500m);
        var transaction = account.Transactions.Single();

        _transactionRepository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);

        var result = await _handler.Handle(new GetTransactionQuery(transaction.Id), CancellationToken.None);

        result.Id.Should().Be(transaction.Id);
        result.AccountId.Should().Be(account.Id);
        result.Amount.Should().Be(500m);
        result.Type.Should().Be("Deposit");
    }

    [Fact]
    public async Task Handle_NonExistentTransaction_ThrowsTransactionNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(unknownId, Arg.Any<CancellationToken>()).ReturnsNull();

        var act = async () => await _handler.Handle(new GetTransactionQuery(unknownId), CancellationToken.None);

        await act.Should().ThrowAsync<TransactionNotFoundException>()
            .WithMessage($"*{unknownId}*");
    }
}
