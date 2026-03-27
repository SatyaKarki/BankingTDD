using MediatR;
using TDDTest.Domain.Exceptions;
using TDDTest.Domain.Interfaces;

namespace TDDTest.Application.Transactions.Commands.TransferBalance;

public sealed class TransferBalanceCommandHandler : IRequestHandler<TransferBalanceCommand, TransferResultDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferBalanceCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferResultDto> Handle(TransferBalanceCommand request, CancellationToken cancellationToken)
    {
        var sourceAccount = await _accountRepository.GetByIdAsync(request.SourceAccountId, cancellationToken)
            ?? throw new AccountNotFoundException(request.SourceAccountId);

        var destinationAccount = await _accountRepository.GetByIdAsync(request.DestinationAccountId, cancellationToken)
            ?? throw new AccountNotFoundException(request.DestinationAccountId);

        var debitTx = sourceAccount.DebitForTransfer(
            request.Amount,
            $"Transfer to {destinationAccount.AccountNumber}: {request.Description}");

        var creditTx = destinationAccount.CreditFromTransfer(
            request.Amount,
            $"Transfer from {sourceAccount.AccountNumber}: {request.Description}",
            debitTx.Id);

        _accountRepository.Update(sourceAccount);
        _accountRepository.Update(destinationAccount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransferResultDto(
            sourceAccount.Id,
            destinationAccount.Id,
            request.Amount,
            debitTx.Id,
            creditTx.Id,
            sourceAccount.Balance,
            destinationAccount.Balance,
            DateTime.UtcNow);
    }
}
