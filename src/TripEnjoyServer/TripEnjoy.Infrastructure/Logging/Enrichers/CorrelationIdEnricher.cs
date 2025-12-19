using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace TripEnjoy.Infrastructure.Logging.Enrichers;

/// <summary>
/// Enricher to add correlation ID to all log events for request tracking
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = GetCorrelationId();
        var property = propertyFactory.CreateProperty(CorrelationIdPropertyName, correlationId);
        logEvent.AddPropertyIfAbsent(property);
    }

    private string GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext == null)
        {
            return Guid.NewGuid().ToString();
        }

        // Try to get correlation ID from headers
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            return correlationId.ToString();
        }

        // Try to get from TraceIdentifier
        if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
        {
            return httpContext.TraceIdentifier;
        }

        // Generate new correlation ID
        var newCorrelationId = Guid.NewGuid().ToString();
        httpContext.Items["CorrelationId"] = newCorrelationId;
        return newCorrelationId;
    }
}
