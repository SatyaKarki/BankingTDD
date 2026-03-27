using MediatR;
using TDDTest.Application.DTOs;

namespace TDDTest.Application.Transactions.Queries.GetAccountTransactions;

public sealed record GetAccountTransactionsQuery(
    Guid AccountId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<TransactionDto>>;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
