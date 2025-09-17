using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.Common.Models;
    using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Domain.Property.Entities
{
    public class PropertyImage : Entity<PropertyImageId>
    {
        public PropertyId PropertyId { get; private set; }
        public string ImageUrl { get; private set; }
        public bool IsMain { get; private set; }
        public DateTime UploadAt { get;  private set;}

        private PropertyImage() : base(PropertyImageId.CreateUnique())
        {
            PropertyId = null!;
            ImageUrl = null!;
        }
        public PropertyImage(PropertyImageId id, PropertyId propertyId, string imageUrl) : base(id)
        {
            PropertyId = propertyId;
            ImageUrl = imageUrl;
            IsMain = false;
            UploadAt = DateTime.UtcNow;
        }
        public void SetAsMain()
        {
            IsMain = true;
        }
        public void SetAsNotMain()
        {
            IsMain = false;
        }
    }
}