using Serilog.Core;
using Serilog.Events;
using System.Text.RegularExpressions;

namespace TripEnjoy.Infrastructure.Logging.Enrichers;

/// <summary>
/// Enricher to mask sensitive data in log messages
/// </summary>
public class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    private static readonly Regex[] SensitivePatterns = new[]
    {
        new Regex(@"password[""']?\s*[:=]\s*[""']?([^""',}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"token[""']?\s*[:=]\s*[""']?([^""',}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"apikey[""']?\s*[:=]\s*[""']?([^""',}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"secret[""']?\s*[:=]\s*[""']?([^""',}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"\b\d{13,19}\b", RegexOptions.Compiled), // Credit card numbers
        new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled), // SSN
    };

    private const string MaskValue = "***MASKED***";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("Message", out var messageProperty))
        {
            var maskedMessage = MaskSensitiveData(messageProperty.ToString());
            if (maskedMessage != messageProperty.ToString())
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Message", maskedMessage));
            }
        }
    }

    private static string MaskSensitiveData(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        foreach (var pattern in SensitivePatterns)
        {
            message = pattern.Replace(message, $"$0{MaskValue}");
        }

        return message;
    }
}
