using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class PartnerId : ValueObject
    {
        public Guid Id { get; private set; }
        public PartnerId(Guid id)
        {
            Id = id;
        }
        public static PartnerId Create(Guid id)
        {
            return new PartnerId(id);
        }
        public static PartnerId CreateUnique()
        {
            return new PartnerId(Guid.NewGuid());
        }
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
