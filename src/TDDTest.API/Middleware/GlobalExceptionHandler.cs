using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TDDTest.Domain.Exceptions;

namespace TDDTest.API.Middleware;

/// <summary>
/// Global exception handler that maps domain and validation exceptions to
/// RFC 7807 Problem Details responses.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, title, detail, errors) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (errors is not null)
            problemDetails.Extensions["errors"] = errors;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static (int statusCode, string title, string detail, object? errors) MapException(Exception exception)
        => exception switch
        {
            AccountNotFoundException ex =>
                (StatusCodes.Status404NotFound, "Account Not Found", ex.Message, null),

            TransactionNotFoundException ex =>
                (StatusCodes.Status404NotFound, "Transaction Not Found", ex.Message, null),

            InsufficientFundsException ex =>
                (StatusCodes.Status422UnprocessableEntity, "Insufficient Funds", ex.Message, null),

            AccountOperationException ex =>
                (StatusCodes.Status422UnprocessableEntity, "Account Operation Failed", ex.Message, null),

            DomainValidationException ex =>
                (StatusCodes.Status400BadRequest, "Validation Error", ex.Message, null),

            ValidationException ex =>
                (StatusCodes.Status400BadRequest, "Validation Failed", "One or more validation errors occurred.",
                    ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray())),

            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error",
                "An unexpected error occurred.", null)
        };
}
