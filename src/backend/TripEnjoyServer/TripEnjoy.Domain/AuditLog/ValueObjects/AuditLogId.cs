using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.AuditLog.ValueObjects
{
    public class AuditLogId : ValueObject
    {

        public Guid Id {get ; private set;}
        public AuditLogId(Guid id)
        {
            Id = id;
        }
        public static AuditLogId Create(Guid id)
        {
            return new AuditLogId(id);
        }
        public static AuditLogId CreateUnique()
        {
            return new AuditLogId(Guid.NewGuid());
        }
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}