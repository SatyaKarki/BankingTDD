using MediatR;
using TDDTest.Application.Common.Mapping;
using TDDTest.Application.DTOs;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Accounts.Queries.GetBalance;

public sealed class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, BalanceDto>
{
    private readonly IAccountRepository _accountRepository;

    public GetBalanceQueryHandler(IAccountRepository accountRepository)
        => _accountRepository = accountRepository;

    public async Task<BalanceDto> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new AccountNotFoundException(request.AccountId);

        return account.ToBalanceDto();
    }
}
