using System;
using System.Collections.Generic;

namespace TripEnjoy.ShareKernel.Dtos;

public class PropertyDto
{
    public Guid Id { get; set; }
    public Guid PropertyTypeId { get; set; }
    public string PropertyTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PropertyImageDto> Images { get; set; } = new();
}
