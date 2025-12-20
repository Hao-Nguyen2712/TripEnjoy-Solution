using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class SettlementId : ValueObject
    {
        public Guid Id { get; private set; }

        public SettlementId(Guid id)
        {
            Id = id;
        }

        public static SettlementId Create(Guid id)
        {
            return new SettlementId(id);
        }

        public static SettlementId CreateUnique()
        {
            return new SettlementId(Guid.NewGuid());
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
