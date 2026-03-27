namespace TDDTest.Domain.Exceptions;

/// <summary>
/// Thrown when a domain validation rule is violated (invalid input or invariant breach).
/// </summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}
