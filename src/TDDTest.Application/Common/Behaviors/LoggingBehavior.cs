using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TDDTest.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request execution time and surfaces
/// slow requests (> 500 ms) as warnings.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowRequestThresholdMs = 500;
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
            _logger.LogWarning("Slow request detected: {RequestName} took {ElapsedMs} ms", requestName, sw.ElapsedMilliseconds);
        else
            _logger.LogInformation("Handled {RequestName} in {ElapsedMs} ms", requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
