using MediatR;
using Microsoft.AspNetCore.Mvc;
using TDDTest.Application.Accounts.Commands.CreateAccount;
using TDDTest.Application.Accounts.Queries.GetAccountById;
using TDDTest.Application.Accounts.Queries.GetBalance;
using TDDTest.Application.Transactions.Queries.GetAccountTransactions;
using TDDTest.API.Models;
using TDDTest.API.Models.Requests;

namespace TDDTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Creates a new bank account.</summary>
    /// <response code="201">Account created successfully.</response>
    /// <response code="400">Validation error in request body.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.AccountDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(request.OwnerName, request.Email, request.InitialDeposit);
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetAccount),
            new { accountId = result.Id },
            ApiResponse<Application.DTOs.AccountDto>.Ok(result, "Account created successfully."));
    }

    /// <summary>Retrieves account details by ID.</summary>
    /// <response code="200">Account found.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("{accountId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAccountByIdQuery(accountId), cancellationToken);
        return Ok(ApiResponse<Application.DTOs.AccountDto>.Ok(result));
    }

    /// <summary>Retrieves the current balance for an account.</summary>
    /// <response code="200">Balance retrieved.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("{accountId:guid}/balance")]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.BalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBalanceQuery(accountId), cancellationToken);
        return Ok(ApiResponse<Application.DTOs.BalanceDto>.Ok(result));
    }

    /// <summary>Retrieves paginated transaction history for an account.</summary>
    /// <response code="200">Transactions retrieved.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("{accountId:guid}/transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Application.DTOs.TransactionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountTransactions(
        Guid accountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAccountTransactionsQuery(accountId, pageNumber, pageSize),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<Application.DTOs.TransactionDto>>.Ok(result));
    }
}
