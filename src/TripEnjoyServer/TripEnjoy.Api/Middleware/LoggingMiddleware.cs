using System.Diagnostics;

namespace TripEnjoy.Api.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            
            _logger.LogInformation(
                "Request Starting: HTTP {RequestMethod} {RequestPath} from IP {IpAddress}",
                requestMethod, requestPath, ipAddress);

            try
            {
                await _next(context); // Call the next middleware in the pipeline
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "Request Finished: HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    requestMethod, requestPath, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
