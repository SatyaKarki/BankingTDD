using MediatR;
using Microsoft.AspNetCore.Mvc;
using TDDTest.Application.Transactions.Commands.TransferBalance;
using TDDTest.Application.Transactions.Queries.GetTransaction;
using TDDTest.API.Models;
using TDDTest.API.Models.Requests;

namespace TDDTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Transfers funds between two accounts.</summary>
    /// <response code="200">Transfer completed successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Source or destination account not found.</response>
    /// <response code="422">Insufficient funds or account operation error.</response>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse<TransferResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new TransferBalanceCommand(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount,
            request.Description);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<TransferResultDto>.Ok(result, "Transfer completed successfully."));
    }

    /// <summary>Retrieves a specific transaction by ID.</summary>
    /// <response code="200">Transaction found.</response>
    /// <response code="404">Transaction not found.</response>
    [HttpGet("{transactionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransactionQuery(transactionId), cancellationToken);
        return Ok(ApiResponse<Application.DTOs.TransactionDto>.Ok(result));
    }
}
