using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TripEnjoy.Application.Interfaces.Logging;

namespace TripEnjoy.Infrastructure.Logging;

/// <summary>
/// Implementation of enhanced logging service with structured logging
/// </summary>
public class LogService : ILogService
{
    private readonly ILogger<LogService> _logger;

    public LogService(ILogger<LogService> logger)
    {
        _logger = logger;
    }

    public void LogInfo(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogPerformance(string operationName, long elapsedMilliseconds, Dictionary<string, object>? additionalData = null)
    {
        var logData = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["ElapsedMilliseconds"] = elapsedMilliseconds,
            ["EventType"] = "Performance"
        };

        if (additionalData != null)
        {
            foreach (var kvp in additionalData)
            {
                logData[kvp.Key] = kvp.Value;
            }
        }

        _logger.LogInformation("Performance: {OperationName} completed in {ElapsedMilliseconds}ms. {@PerformanceData}",
            operationName, elapsedMilliseconds, logData);
    }

    public void LogSecurity(string eventType, string message, Dictionary<string, object>? additionalData = null)
    {
        var logData = new Dictionary<string, object>
        {
            ["EventType"] = "Security",
            ["SecurityEventType"] = eventType,
            ["Message"] = message
        };

        if (additionalData != null)
        {
            foreach (var kvp in additionalData)
            {
                logData[kvp.Key] = kvp.Value;
            }
        }

        _logger.LogWarning("Security Event: {SecurityEventType} - {Message}. {@SecurityData}",
            eventType, message, logData);
    }

    public void LogBusinessEvent(string eventName, Dictionary<string, object>? eventData = null)
    {
        var logData = new Dictionary<string, object>
        {
            ["EventType"] = "Business",
            ["EventName"] = eventName
        };

        if (eventData != null)
        {
            foreach (var kvp in eventData)
            {
                logData[kvp.Key] = kvp.Value;
            }
        }

        _logger.LogInformation("Business Event: {EventName}. {@BusinessEventData}",
            eventName, logData);
    }
}
