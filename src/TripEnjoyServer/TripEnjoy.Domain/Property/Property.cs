using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.Entities;
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
        public readonly List<PropertyImage> _propertyImages = new();
        public IReadOnlyList<PropertyImage> PropertyImages => _propertyImages.AsReadOnly();
        
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

        public Result Update(PropertyTypeId propertyTypeId, string name, string address, string city, string country, string? description = null, double? latitude = null, double? longitude = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure(DomainError.Property.NameIsRequired);
            }

            PropertyTypeId = propertyTypeId;
            Name = name;
            Address = address;
            City = city;
            Country = country;
            Description = description;
            Latitude = latitude;
            Longitude = longitude;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result AddImage(string imageUrl, bool isCover)
        {
            var propertyImage = new PropertyImage(PropertyImageId.CreateUnique(), Id, imageUrl);

            if (isCover)
            {
                // Set all other images to not be the main one
                _propertyImages.ForEach(img => img.SetAsNotMain());
                propertyImage.SetAsMain();
            }
            else if (!_propertyImages.Any(img => img.IsMain))
            {
                // If no other image is the main one, make this the main one
                propertyImage.SetAsMain();
            }

            _propertyImages.Add(propertyImage);
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result RemoveImage(PropertyImageId imageId)
        {
            var imageToRemove = _propertyImages.FirstOrDefault(img => img.Id == imageId);
            if (imageToRemove == null)
            {
                return Result.Failure(DomainError.Property.ImageNotFound);
            }

            _propertyImages.Remove(imageToRemove);

            // If the removed image was the main one, and there are other images, set the first one as the new main
            if (imageToRemove.IsMain && _propertyImages.Any())
            {
                _propertyImages.First().SetAsMain();
            }

            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result SetCoverImage(PropertyImageId imageId)
        {
            var imageToSetAsMain = _propertyImages.FirstOrDefault(img => img.Id == imageId);
            if (imageToSetAsMain == null)
            {
                return Result.Failure(DomainError.Property.ImageNotFound);
            }

            // Reset all other images
            _propertyImages.ForEach(img => img.SetAsNotMain());
            
            // Set the new main image
            imageToSetAsMain.SetAsMain();

            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Approve()
        {
            if (Status == PropertyEnum.Active)
            {
                return Result.Failure(new Error("Property.AlreadyApproved", "Property is already approved.", ErrorType.Conflict));
            }

            Status = PropertyEnum.Active;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Reject()
        {
            if (Status == PropertyEnum.Inactive)
            {
                return Result.Failure(new Error("Property.AlreadyRejected", "Property is already rejected.", ErrorType.Conflict));
            }

            Status = PropertyEnum.Inactive;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Deactivate()
        {
            if (Status == PropertyEnum.Inactive)
            {
                return Result.Failure(new Error("Property.AlreadyInactive", "Property is already inactive.", ErrorType.Conflict));
            }

            Status = PropertyEnum.Inactive;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
