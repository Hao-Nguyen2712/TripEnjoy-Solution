using System;

namespace TripEnjoy.ShareKernel.Dtos;

public class RoomTypeImageDto
{
    public Guid Id { get; set; }
    public Guid RoomTypeId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public DateTime UploadedAt { get; set; }
}
