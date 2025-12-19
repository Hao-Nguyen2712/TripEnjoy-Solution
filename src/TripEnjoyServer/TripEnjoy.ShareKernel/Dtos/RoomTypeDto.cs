using System;
using System.Collections.Generic;

namespace TripEnjoy.ShareKernel.Dtos;

public class RoomTypeDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public int TotalQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<RoomTypeImageDto> Images { get; set; } = new();
}
