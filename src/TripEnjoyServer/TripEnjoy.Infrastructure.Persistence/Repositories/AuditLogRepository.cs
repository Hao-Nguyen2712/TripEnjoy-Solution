using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.AuditLog;

namespace TripEnjoy.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(TripEnjoyDbContext dbContext) : base(dbContext)
    {
    }
}
