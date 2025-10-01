using TripEnjoy.Domain.AuditLog;

namespace TripEnjoy.Application.Interfaces.Persistence;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    // Add specific query methods here if needed in the future
}
