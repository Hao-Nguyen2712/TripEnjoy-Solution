using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Text;
using TripEnjoy.Application.Interfaces.Logging;

namespace TripEnjoy.Api.Middleware;

/// <summary>
/// Enhanced logging middleware with correlation ID, request/response logging, and performance tracking
/// </summary>
public class EnhancedLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogService _logService;
    private readonly IConfiguration _configuration;

    public EnhancedLoggingMiddleware(RequestDelegate next, ILogService logService, IConfiguration configuration)
    {
        _next = next;
        _logService = logService;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or extract correlation ID
        var correlationId = GetOrCreateCorrelationId(context);
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Log request start
        _logService.LogInfo(
            "Request Starting: HTTP {RequestMethod} {RequestPath} from IP {IpAddress} [CorrelationId: {CorrelationId}]",
            requestMethod, requestPath, ipAddress, correlationId);

        // Optionally log request body (only for non-GET requests and in development)
        if (_configuration.GetValue<bool>("Logging:LogRequestBody", false) && requestMethod != "GET")
        {
            await LogRequestBodyAsync(context, correlationId);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log request completion with performance data
            var performanceData = new Dictionary<string, object>
            {
                ["RequestMethod"] = requestMethod,
                ["RequestPath"] = requestPath.ToString(),
                ["StatusCode"] = context.Response.StatusCode,
                ["IpAddress"] = ipAddress,
                ["CorrelationId"] = correlationId
            };

            _logService.LogPerformance(
                $"HTTP {requestMethod} {requestPath}",
                stopwatch.ElapsedMilliseconds,
                performanceData);

            // Log slow requests as warnings
            var slowRequestThreshold = _configuration.GetValue<int>("Logging:SlowRequestThresholdMs", 5000);
            if (stopwatch.ElapsedMilliseconds > slowRequestThreshold)
            {
                _logService.LogWarning(
                    "Slow Request Detected: HTTP {RequestMethod} {RequestPath} took {ElapsedMilliseconds}ms [CorrelationId: {CorrelationId}]",
                    requestMethod, requestPath, stopwatch.ElapsedMilliseconds, correlationId);
            }
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Check for correlation ID in request headers
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId) &&
            !string.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        // Use TraceIdentifier if available
        if (!string.IsNullOrEmpty(context.TraceIdentifier))
        {
            return context.TraceIdentifier;
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString();
    }

    private async Task LogRequestBodyAsync(HttpContext context, string correlationId)
    {
        context.Request.EnableBuffering();
        
        using var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (!string.IsNullOrEmpty(body))
        {
            // Note: This will be masked by SensitiveDataMaskingEnricher if contains sensitive data
            _logService.LogDebug(
                "Request Body for {RequestMethod} {RequestPath} [CorrelationId: {CorrelationId}]: {RequestBody}",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                body);
        }
    }
}

public static class EnhancedLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseEnhancedLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<EnhancedLoggingMiddleware>();
    }
}
