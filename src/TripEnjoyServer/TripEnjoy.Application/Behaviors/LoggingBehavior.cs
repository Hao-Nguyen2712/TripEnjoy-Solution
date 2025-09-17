using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TripEnjoy.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling request {RequestName}. Request data: {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        _logger.LogInformation("Handled request {RequestName} in {ElapsedMilliseconds}ms.", requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
