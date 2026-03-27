namespace TDDTest.API.Models.Requests;

/// <summary>Request body for transferring funds between accounts.</summary>
public sealed record TransferBalanceRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description);
