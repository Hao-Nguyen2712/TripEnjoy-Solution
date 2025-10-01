using TripEnjoy.Application.Common.Models;

namespace TripEnjoy.Application.Common.Interfaces;


public interface IAuditLogService
{
    Task CreateAuditLogAsync(AuditLogEntry logEntry, CancellationToken cancellationToken);
}
