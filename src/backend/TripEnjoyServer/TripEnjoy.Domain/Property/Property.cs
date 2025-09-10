using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.Enums;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.PropertyType.ValueObjects;

namespace TripEnjoy.Domain.Property
{
    public class Property : AggregateRoot<PropertyId>
    {
        public PartnerId PartnerId { get; private set; }
        public PropertyTypeId PropertyTypeId { get; private set; }
        public Partner Partner { get; private set; }
        public Domain.PropertyType.PropertyType PropertyType { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string Country { get; private set; }
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }
        public string Status { get; private set; }
        public decimal? AverageRating { get; private set; }
        public int ReviewCount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        private Property() : base(PropertyId.CreateUnique())
        {
            PartnerId = null!;
            PropertyTypeId = null!;
            PropertyType = null!;
            Partner = null!;
            Name = null!;
            Address = null!;
            City = null!;
            Country = null!;
            Status = null!;
        }

        public Property(PropertyId id, PartnerId partnerId, PropertyTypeId propertyTypeId, string name, string address, string city, string country, string? description = null, double? latitude = null, double? longitude = null) : base(id)
        {
            PartnerId = partnerId;
            PropertyTypeId = propertyTypeId;
            Name = name;
            Description = description;
            Address = address;
            City = city;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
            Status = PropertyEnum.WaitingForApproval.ToString();
            AverageRating = 0;
            ReviewCount = 0;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
        }

        public static Result<Property> Create(PartnerId partnerId, PropertyTypeId propertyTypeId, string name, string address, string city, string country, string? description = null, double? latitude = null, double? longitude = null)
        {
            // Add validation here, for example:
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result<Property>.Failure(DomainError.Property.NameIsRequired);
            }
            var property = new Property(PropertyId.CreateUnique(), partnerId, propertyTypeId, name, address, city, country, description, latitude, longitude);
            return Result<Property>.Success(property);
        }
    }
}
