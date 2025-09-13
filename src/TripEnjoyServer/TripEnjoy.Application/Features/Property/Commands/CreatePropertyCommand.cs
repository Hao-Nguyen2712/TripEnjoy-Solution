using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.PropertyType.ValueObjects;

namespace TripEnjoy.Application.Features.Property.Commands
{
    public record CreatePropertyCommand
    {
        public PropertyTypeId PropertyTypeId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string? Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}