using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using TDDTest.Application.Common.Behaviors;

namespace TDDTest.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    public sealed record TestRequest(string Value) : IRequest<string>;

    private static ValidationBehavior<TestRequest, string> CreateBehavior(
        params IValidator<TestRequest>[] validators)
        => new(validators);

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = CreateBehavior();
        var nextCalled = false;
        RequestHandlerDelegate<string> next = (ct) => { nextCalled = true; return Task.FromResult("ok"); };

        await behavior.Handle(new TestRequest("value"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PassingValidation_CallsNext()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        var behavior = CreateBehavior(validator);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = (ct) => { nextCalled = true; return Task.FromResult("ok"); };

        await behavior.Handle(new TestRequest("valid"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FailingValidation_ThrowsValidationException()
    {
        var failures = new[] { new FluentValidation.Results.ValidationFailure("Value", "Value is required.") };
        var validationResult = new FluentValidation.Results.ValidationResult(failures);

        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        var behavior = CreateBehavior(validator);
        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("should not reach");

        var act = async () => await behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Value is required*");
    }

    [Fact]
    public async Task Handle_MultipleValidators_AggregatesAllErrors()
    {
        var failure1 = new FluentValidation.Results.ValidationFailure("Value", "Error 1.");
        var failure2 = new FluentValidation.Results.ValidationFailure("Value", "Error 2.");

        var v1 = Substitute.For<IValidator<TestRequest>>();
        v1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult(new[] { failure1 }));

        var v2 = Substitute.For<IValidator<TestRequest>>();
        v2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult(new[] { failure2 }));

        var behavior = CreateBehavior(v1, v2);
        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("no");

        var act = async () => await behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
    }
}
