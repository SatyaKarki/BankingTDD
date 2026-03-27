namespace TDDTest.Domain.Exceptions;

public sealed class InsufficientFundsException : DomainException
{
    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }

    public InsufficientFundsException(decimal requestedAmount, decimal availableBalance)
        : base($"Insufficient funds. Requested: {requestedAmount:C}, Available: {availableBalance:C}.")
    {
        RequestedAmount = requestedAmount;
        AvailableBalance = availableBalance;
    }
}
