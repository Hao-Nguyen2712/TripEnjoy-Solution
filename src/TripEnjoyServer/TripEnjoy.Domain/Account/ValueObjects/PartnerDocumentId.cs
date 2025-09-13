using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class PartnerDocumentId : ValueObject
    {
        public Guid Id { get; private set; }

        public PartnerDocumentId(Guid id)
        {
            Id = id;
        }

        public static PartnerDocumentId Create(Guid id)
        {
            return new PartnerDocumentId(id);
        }
        public static PartnerDocumentId CreateUnique()
        {
            return new PartnerDocumentId(Guid.NewGuid());
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}