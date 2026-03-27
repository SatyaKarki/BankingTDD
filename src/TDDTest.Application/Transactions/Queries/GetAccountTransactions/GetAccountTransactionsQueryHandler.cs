using MediatR;
using TDDTest.Application.Common.Mapping;
using TDDTest.Application.DTOs;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Transactions.Queries.GetAccountTransactions;

public sealed class GetAccountTransactionsQueryHandler
    : IRequestHandler<GetAccountTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetAccountTransactionsQueryHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<PagedResult<TransactionDto>> Handle(
        GetAccountTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await _accountRepository.ExistsAsync(request.AccountId, cancellationToken))
            throw new AccountNotFoundException(request.AccountId);

        var allTransactions = await _transactionRepository
            .GetByAccountIdAsync(request.AccountId, cancellationToken);

        var totalCount = allTransactions.Count;

        var paged = allTransactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => t.ToDto())
            .ToList();

        return new PagedResult<TransactionDto>(paged, totalCount, request.PageNumber, request.PageSize);
    }
}
