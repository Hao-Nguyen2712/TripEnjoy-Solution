using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class UserId : ValueObject
    {
        public Guid Id { get; private set; }

        public UserId(Guid id)
        {
            Id = id;
        }

        public static UserId Create(Guid id)
        {
            return new UserId(id);
        }

        public static UserId CreateUnique()
        {
            return new UserId(Guid.NewGuid());
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}