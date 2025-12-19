namespace TripEnjoy.Application.Interfaces.Logging;

/// <summary>
/// Enhanced logging service abstraction with structured logging support
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Log informational message with structured data
    /// </summary>
    void LogInfo(string message, params object[] args);

    /// <summary>
    /// Log warning message with structured data
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Log error with exception and structured data
    /// </summary>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Log debug message (only in development)
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Log performance metric
    /// </summary>
    void LogPerformance(string operationName, long elapsedMilliseconds, Dictionary<string, object>? additionalData = null);

    /// <summary>
    /// Log security event (authentication, authorization, etc.)
    /// </summary>
    void LogSecurity(string eventType, string message, Dictionary<string, object>? additionalData = null);

    /// <summary>
    /// Log business event with correlation
    /// </summary>
    void LogBusinessEvent(string eventName, Dictionary<string, object>? eventData = null);
}
