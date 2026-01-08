using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Client.Models;

// Request Models
public class CreateRoomTypeRequest
{
    public Guid PropertyId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public int TotalQuantity { get; set; }
    public string? Description { get; set; }
}

public class UpdateRoomTypeRequest
{
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public int TotalQuantity { get; set; }
    public string? Description { get; set; }
}
