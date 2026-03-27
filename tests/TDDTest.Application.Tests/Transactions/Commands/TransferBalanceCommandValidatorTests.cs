using FluentAssertions;
using FluentValidation.TestHelper;
using TDDTest.Application.Transactions.Commands.TransferBalance;

namespace TDDTest.Application.Tests.Transactions.Commands;

public sealed class TransferBalanceCommandValidatorTests
{
    private readonly TransferBalanceCommandValidator _validator = new();

    private static Guid AnyGuid => Guid.NewGuid();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var src = AnyGuid;
        var dst = AnyGuid;
        var command = new TransferBalanceCommand(src, dst, 100m, "Payment");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SameSourceAndDestination_HasValidationError()
    {
        var id = AnyGuid;
        var command = new TransferBalanceCommand(id, id, 100m, "Self-transfer");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DestinationAccountId)
            .WithErrorMessage("Source and destination accounts must be different.");
    }

    [Fact]
    public void Validate_ZeroAmount_HasValidationError()
    {
        var command = new TransferBalanceCommand(AnyGuid, AnyGuid, 0m, "Zero");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_NegativeAmount_HasValidationError()
    {
        var command = new TransferBalanceCommand(AnyGuid, AnyGuid, -50m, "Negative");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyDescription_HasValidationError(string description)
    {
        var command = new TransferBalanceCommand(AnyGuid, AnyGuid, 100m, description);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_EmptySourceId_HasValidationError()
    {
        var command = new TransferBalanceCommand(Guid.Empty, AnyGuid, 100m, "Transfer");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SourceAccountId);
    }

    [Fact]
    public void Validate_EmptyDestinationId_HasValidationError()
    {
        var command = new TransferBalanceCommand(AnyGuid, Guid.Empty, 100m, "Transfer");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DestinationAccountId);
    }
}
