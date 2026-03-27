using MediatR;
using TDDTest.Application.DTOs;

namespace TDDTest.Application.Accounts.Queries.GetAccountById;

public sealed record GetAccountByIdQuery(Guid AccountId) : IRequest<AccountDto>;
