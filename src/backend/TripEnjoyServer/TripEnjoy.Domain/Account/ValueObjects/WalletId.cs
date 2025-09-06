using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class WalletId : ValueObject
    {
        public Guid Id { get; private set; }

        public WalletId(Guid id)
        {
            Id = id;
        }

        public static WalletId Create(Guid id)
        {
            return new WalletId(id);
        }

        public static WalletId CreateUnique()
        {
            return new WalletId(Guid.NewGuid());
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
