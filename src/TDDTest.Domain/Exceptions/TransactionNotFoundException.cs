namespace TDDTest.Domain.Exceptions;

public sealed class TransactionNotFoundException : DomainException
{
    public Guid TransactionId { get; }

    public TransactionNotFoundException(Guid transactionId)
        : base($"Transaction with ID '{transactionId}' was not found.")
    {
        TransactionId = transactionId;
    }
}
