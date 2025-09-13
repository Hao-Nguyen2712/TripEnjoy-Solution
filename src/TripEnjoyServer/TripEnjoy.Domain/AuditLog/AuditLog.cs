using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.AuditLog.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.AuditLog
{
    public class AuditLog : AggregateRoot<AuditLogId>
    {
        public AccountId AccountId { get; private set; }
        public Domain.Account.Account Account { get; private set; } = null!;
        public string Action { get; private set; }
        public string EntityName { get; private set; }
        public string EntityId { get; private set; }
        public string OldValue { get; private set; }
        public string NewValue { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private AuditLog() : base(AuditLogId.CreateUnique())
        {
            AccountId = null!;
            Action = null!;
            EntityName = null!;
            EntityId = null!;
            OldValue = null!;
            NewValue = null!;
        }

        public AuditLog(AuditLogId id, AccountId accountId, string action, string entityName, string entityId, string oldValue, string newValue) : base(id)
        {
            AccountId = accountId;
            Action = action;
            EntityName = entityName;
            EntityId = entityId;
            OldValue = oldValue;
            NewValue = newValue;
            CreatedAt = DateTime.UtcNow;
        }
    }
}