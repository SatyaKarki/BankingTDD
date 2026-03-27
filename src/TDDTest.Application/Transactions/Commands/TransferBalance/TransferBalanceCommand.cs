using MediatR;
using TDDTest.Application.DTOs;

namespace TDDTest.Application.Transactions.Commands.TransferBalance;

public sealed record TransferBalanceCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description) : IRequest<TransferResultDto>;

public sealed record TransferResultDto(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    Guid SourceTransactionId,
    Guid DestinationTransactionId,
    decimal SourceBalanceAfter,
    decimal DestinationBalanceAfter,
    DateTime TransferredAt);
