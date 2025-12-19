namespace TripEnjoy.Client.Models;

// DTOs
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

public class RoomTypeImageDto
{
    public Guid Id { get; set; }
    public Guid RoomTypeId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public DateTime UploadedAt { get; set; }
}

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
