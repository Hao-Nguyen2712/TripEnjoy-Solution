namespace TripEnjoy.Application.Common.Models;

/// <summary>
/// A Data Transfer Object used to pass information to the background audit logging service.
/// </summary>
public class AuditLogEntry
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g., "CreatePropertyCommand"
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; } // Will contain the serialized command
}
