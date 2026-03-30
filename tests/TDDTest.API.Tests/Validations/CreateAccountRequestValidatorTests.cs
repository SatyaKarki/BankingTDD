using FluentAssertions;
using FluentValidation.TestHelper;
using TDDTest.API.Models.Requests;
using TDDTest.API.Validations;

namespace TDDTest.API.Tests.Validations;

public sealed class CreateAccountRequestValidatorTests
{
    private readonly CreateAccountRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: "john.doe@example.com",
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidRequestWithZeroDeposit_ShouldNotHaveValidationErrors()
    {
        var request = new CreateAccountRequest(
            OwnerName: "Jane Smith",
            Email: "jane.smith@example.com",
            InitialDeposit: 0m);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyOwnerName_ShouldHaveValidationError(string ownerName)
    {
        var request = new CreateAccountRequest(
            OwnerName: ownerName,
            Email: "test@example.com",
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OwnerName)
            .WithErrorMessage("Owner name is required.");
    }

    [Fact]
    public void Validate_OwnerNameExceedsMaxLength_ShouldHaveValidationError()
    {
        var longName = new string('A', 101);
        var request = new CreateAccountRequest(
            OwnerName: longName,
            Email: "test@example.com",
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OwnerName)
            .WithErrorMessage("Owner name must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_OwnerNameAtMaxLength_ShouldNotHaveValidationError()
    {
        var maxLengthName = new string('A', 100);
        var request = new CreateAccountRequest(
            OwnerName: maxLengthName,
            Email: "test@example.com",
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.OwnerName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string email)
    {
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: email,
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user")]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: email,
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("john.doe@example.co.uk")]
    [InlineData("jane_smith@test-domain.org")]
    [InlineData("test+alias@example.com")]
    public void Validate_ValidEmailFormats_ShouldNotHaveValidationError(string email)
    {
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: email,
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailExceedsMaxLength_ShouldHaveValidationError()
    {
        var longEmail = new string('a', 246) + "@example.com";
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: longEmail,
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 255 characters.");
    }

    [Fact]
    public void Validate_EmailAtMaxLength_ShouldNotHaveValidationError()
    {
        var maxLengthEmail = new string('a', 243) + "@example.com";
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: maxLengthEmail,
            InitialDeposit: 100m);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-1000.50)]
    public void Validate_NegativeInitialDeposit_ShouldHaveValidationError(decimal initialDeposit)
    {
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: "john.doe@example.com",
            InitialDeposit: initialDeposit);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.InitialDeposit)
            .WithErrorMessage("Initial deposit must be greater than or equal to 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000.50)]
    [InlineData(999999.99)]
    public void Validate_ValidInitialDeposit_ShouldNotHaveValidationError(decimal initialDeposit)
    {
        var request = new CreateAccountRequest(
            OwnerName: "John Doe",
            Email: "john.doe@example.com",
            InitialDeposit: initialDeposit);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.InitialDeposit);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        var request = new CreateAccountRequest(
            OwnerName: "",
            Email: "invalid-email",
            InitialDeposit: -10m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OwnerName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.InitialDeposit);
    }

    [Fact]
    public void Validate_AllFieldsInvalid_ShouldHaveThreeValidationErrors()
    {
        var request = new CreateAccountRequest(
            OwnerName: "",
            Email: "not-an-email",
            InitialDeposit: -50m);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }
}
