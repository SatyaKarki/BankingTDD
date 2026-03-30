using FluentValidation;
using TDDTest.API.Models.Requests;

namespace TDDTest.API.Validations;

/// <summary>Validator for CreateAccountRequest.</summary>
public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.OwnerName)
            .NotEmpty()
            .WithMessage("Owner name is required.")
            .MaximumLength(100)
            .WithMessage("Owner name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.InitialDeposit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial deposit must be greater than or equal to 0.");
    }
}
