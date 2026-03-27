namespace TDDTest.Domain.Exceptions;

public sealed class AccountOperationException : DomainException
{
    public AccountOperationException(string message) : base(message) { }
}
