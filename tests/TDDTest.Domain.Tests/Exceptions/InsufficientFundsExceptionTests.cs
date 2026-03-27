using FluentAssertions;
using TDDTest.Domain.Exceptions;

namespace TDDTest.Domain.Tests.Exceptions;

public sealed class InsufficientFundsExceptionTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var ex = new InsufficientFundsException(500m, 100m);

        ex.RequestedAmount.Should().Be(500m);
        ex.AvailableBalance.Should().Be(100m);
        ex.Message.Should().Contain("500").And.Contain("100");
    }

    [Fact]
    public void InheritanceChain_IsCorrect()
    {
        var ex = new InsufficientFundsException(100m, 50m);
        ex.Should().BeAssignableTo<DomainException>();
        ex.Should().BeAssignableTo<Exception>();
    }
}
