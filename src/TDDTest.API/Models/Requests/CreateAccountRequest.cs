namespace TDDTest.API.Models.Requests;

/// <summary>Request body for creating a new bank account.</summary>
public sealed record CreateAccountRequest(
    string OwnerName,
    string Email,
    decimal InitialDeposit = 0m);
