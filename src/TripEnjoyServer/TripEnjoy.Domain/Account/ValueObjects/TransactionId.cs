using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class TransactionId : ValueObject
    {
        public Guid Id { get; private set; }

        public TransactionId(Guid id)
        {
            Id = id;
        }

        public static TransactionId Create(Guid id)
        {
            return new TransactionId(id);
        }

        public static TransactionId CreateUnique()
        {
            return new TransactionId(Guid.NewGuid());
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
