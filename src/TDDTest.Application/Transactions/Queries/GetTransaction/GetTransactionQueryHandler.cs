using MediatR;
using TDDTest.Application.Common.Mapping;
using TDDTest.Application.DTOs;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionQueryHandler(ITransactionRepository transactionRepository)
        => _transactionRepository = transactionRepository;

    public async Task<TransactionDto> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new TransactionNotFoundException(request.TransactionId);

        return transaction.ToDto();
    }
}
