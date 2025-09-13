using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.PropertyType.ValueObjects
{
    public class PropertyTypeId : ValueObject
    {
        public Guid Id { get; private set; }
        public PropertyTypeId(Guid id)
        {
            Id = id;
        }
        public static PropertyTypeId CreateUnique()
        {
            return new PropertyTypeId(Guid.NewGuid());
        }
        public static PropertyTypeId Create(Guid id)
        {
            return new PropertyTypeId(id);
        }
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}