using MediatR;
using TDDTest.Application.DTOs;

namespace TDDTest.Application.Transactions.Queries.GetTransaction;

public sealed record GetTransactionQuery(Guid TransactionId) : IRequest<TransactionDto>;
