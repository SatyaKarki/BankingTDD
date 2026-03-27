namespace TDDTest.Domain.Exceptions;

public sealed class AccountNotFoundException : DomainException
{
    public Guid AccountId { get; }

    public AccountNotFoundException(Guid accountId)
        : base($"Account with ID '{accountId}' was not found.")
    {
        AccountId = accountId;
    }
}
