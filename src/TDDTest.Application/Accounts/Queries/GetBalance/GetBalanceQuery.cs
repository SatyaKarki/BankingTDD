using MediatR;
using TDDTest.Application.DTOs;

namespace TDDTest.Application.Accounts.Queries.GetBalance;

public sealed record GetBalanceQuery(Guid AccountId) : IRequest<BalanceDto>;
