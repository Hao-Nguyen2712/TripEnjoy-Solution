using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Property.ValueObjects
{
    public class PropertyImageId : ValueObject
    {
        public Guid Id { get; private set; }
        public PropertyImageId(Guid id)
        {
            Id = id;
        }
        public static PropertyImageId Create(Guid id)
        {
            return new PropertyImageId(id);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public static PropertyImageId CreateUnique()
        {
            return new PropertyImageId(Guid.NewGuid());
        }
    }
}