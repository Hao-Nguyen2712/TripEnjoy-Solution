using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Application.Common.Models;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.AuditLog;
using TripEnjoy.Domain.AuditLog.ValueObjects;

namespace TripEnjoy.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAuditLogAsync(AuditLogEntry logEntry, CancellationToken cancellationToken)
    {
        // This is a simplified implementation. A more robust solution might involve a separate ICurrentUserService
        // to abstract away the dependency on specific claim types.
        if (!Guid.TryParse(logEntry.UserId, out var accountGuid))
        {
            // Handle cases where the UserId might be "System" or invalid.
            // For now, we'll skip creating a log for these, but you could also log them with a null AccountId.
            return;
        }
        var accountId = AccountId.Create(accountGuid);

        var newAuditLog = new AuditLog(
            AuditLogId.CreateUnique(),
            accountId,
            logEntry.Action,
            logEntry.EntityName,
            logEntry.EntityId ?? string.Empty,
            logEntry.OldValue ?? string.Empty,
            logEntry.NewValue ?? string.Empty
        );

        await _unitOfWork.AuditLogs.AddAsync(newAuditLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
