using MediatR;
using TDDTest.Application.DTOs;

namespace TDDTest.Application.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(
    string OwnerName,
    string Email,
    decimal InitialDeposit) : IRequest<AccountDto>;
