using FluentValidation;

namespace TDDTest.Application.Transactions.Commands.TransferBalance;

public sealed class TransferBalanceCommandValidator : AbstractValidator<TransferBalanceCommand>
{
    public TransferBalanceCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Source account ID is required.");

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty().WithMessage("Destination account ID is required.")
            .NotEqual(x => x.SourceAccountId).WithMessage("Source and destination accounts must be different.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Transfer description is required.")
            .MaximumLength(250).WithMessage("Description must not exceed 250 characters.");
    }
}
