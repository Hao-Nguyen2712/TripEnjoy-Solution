using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TripEnjoy.Application.Interfaces.Logging;

namespace TripEnjoy.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ILogService? _logService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ILogService? logService = null)
    {
        _logger = logger;
        _logService = logService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Use enhanced logging if available, otherwise fall back to standard logging
        if (_logService != null)
        {
            _logService.LogInfo("Handling request {RequestName}. Request data: {@Request}", requestName, request);
        }
        else
        {
            _logger.LogInformation("Handling request {RequestName}. Request data: {@Request}", requestName, request);
        }

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        // Log performance metric with enhanced logging
        if (_logService != null)
        {
            _logService.LogPerformance(
                requestName,
                stopwatch.ElapsedMilliseconds,
                new Dictionary<string, object>
                {
                    ["RequestType"] = typeof(TRequest).FullName ?? requestName,
                    ["ResponseType"] = typeof(TResponse).FullName ?? "Unknown"
                });
        }
        else
        {
            _logger.LogInformation("Handled request {RequestName} in {ElapsedMilliseconds}ms.", requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
