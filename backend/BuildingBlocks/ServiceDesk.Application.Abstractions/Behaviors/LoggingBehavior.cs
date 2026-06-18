using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Application.Abstractions.Behaviors;

/// <summary>
/// Outermost behavior: logs each request's start, outcome, and elapsed time. Failures are
/// logged at warning with the typed error code; unexpected exceptions bubble to the host handler.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startedAt = Stopwatch.GetTimestamp();

        logger.LogInformation("Handling {RequestName}", requestName);

        TResponse response = await next(cancellationToken);

        var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

        if (response.IsSuccess)
        {
            logger.LogInformation("Handled {RequestName} in {ElapsedMs:F1} ms", requestName, elapsedMs);
        }
        else
        {
            logger.LogWarning(
                "Handled {RequestName} in {ElapsedMs:F1} ms with error {ErrorCode} ({ErrorType})",
                requestName,
                elapsedMs,
                response.Error.Code,
                response.Error.Type);
        }

        return response;
    }
}
