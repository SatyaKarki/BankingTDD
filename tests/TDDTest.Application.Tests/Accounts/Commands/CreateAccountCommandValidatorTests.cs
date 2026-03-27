using FluentAssertions;
using FluentValidation.TestHelper;
using TDDTest.Application.Accounts.Commands.CreateAccount;

namespace TDDTest.Application.Tests.Accounts.Commands;

public sealed class CreateAccountCommandValidatorTests
{
    private readonly CreateAccountCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateAccountCommand("Alice", "alice@example.com", 100m);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyOwnerName_HasValidationError(string name)
    {
        var command = new CreateAccountCommand(name, "valid@email.com", 0m);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OwnerName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_InvalidEmail_HasValidationError(string email)
    {
        var command = new CreateAccountCommand("Alice", email, 0m);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_NegativeInitialDeposit_HasValidationError()
    {
        var command = new CreateAccountCommand("Alice", "a@b.com", -1m);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.InitialDeposit);
    }

    [Fact]
    public void Validate_ZeroInitialDeposit_PassesValidation()
    {
        var command = new CreateAccountCommand("Alice", "a@b.com", 0m);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.InitialDeposit);
    }

    [Fact]
    public void Validate_OwnerNameExceedsMaxLength_HasValidationError()
    {
        var longName = new string('A', 151);
        var command = new CreateAccountCommand(longName, "a@b.com", 0m);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OwnerName);
    }
}
