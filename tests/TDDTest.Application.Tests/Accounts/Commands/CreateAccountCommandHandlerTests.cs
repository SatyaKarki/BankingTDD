using FluentAssertions;
using NSubstitute;
using TDDTest.Application.Accounts.Commands.CreateAccount;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Tests.Accounts.Commands;

public sealed class CreateAccountCommandHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
        => _handler = new CreateAccountCommandHandler(_accountRepository, _unitOfWork);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsAccountDto()
    {
        var command = new CreateAccountCommand("Jane Doe", "jane@example.com", 500m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.OwnerName.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
        result.Balance.Should().Be(500m);
        result.Status.Should().Be("Active");
        result.Id.Should().NotBe(Guid.Empty);
        result.AccountNumber.Should().StartWith("ACC-");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddAndSave()
    {
        var command = new CreateAccountCommand("John", "john@test.com", 0m);

        await _handler.Handle(command, CancellationToken.None);

        await _accountRepository.Received(1).AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithZeroDeposit_ReturnsAccountWithZeroBalance()
    {
        var command = new CreateAccountCommand("Minimal", "min@test.com", 0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_PersistedAccount_MatchesReturnedDto()
    {
        Account? capturedAccount = null;
        await _accountRepository.AddAsync(
            Arg.Do<Account>(a => capturedAccount = a),
            Arg.Any<CancellationToken>());

        var command = new CreateAccountCommand("Captured", "cap@test.com", 250m);
        var result = await _handler.Handle(command, CancellationToken.None);

        capturedAccount.Should().NotBeNull();
        capturedAccount!.Id.Should().Be(result.Id);
        capturedAccount.Balance.Should().Be(result.Balance);
    }
}
