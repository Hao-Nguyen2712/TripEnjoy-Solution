using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Property.ValueObjects
{
    public class PropertyId : ValueObject
    {
        public Guid Id { get; private set; }

        public PropertyId(Guid id)
        {
            Id = id;
        }

        public static PropertyId CreateUnique()
        {
            return new PropertyId(Guid.NewGuid());
        }

        public static PropertyId Create(Guid id)
        {
            return new PropertyId(id);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
