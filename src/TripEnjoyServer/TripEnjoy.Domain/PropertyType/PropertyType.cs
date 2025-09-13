using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.PropertyType.Enums;
using TripEnjoy.Domain.PropertyType.ValueObjects;

namespace TripEnjoy.Domain.PropertyType
{
    public class PropertyType : AggregateRoot<PropertyTypeId>
    {
        public string Name { get; private set; }
        public string Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private PropertyType() : base(PropertyTypeId.CreateUnique())
        {
            Name = null!;
            Status = null!;
        }

        public PropertyType(PropertyTypeId id, string name) : base(id)
        {
            Name = name;
            Status = PropertyTypeEnum.Active.ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result UpdateName(string name)
        {
            Name = name;
            UpdatedAt = DateTime.UtcNow;
            return Result<string>.Success("Update Name Successfully");
        }


        public static Result<PropertyType> Create(string name)
        {

            var propertyType = new PropertyType(PropertyTypeId.CreateUnique(), name);
            return Result<PropertyType>.Success(propertyType);
        }
    }
}
