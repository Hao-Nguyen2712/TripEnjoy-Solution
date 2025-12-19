using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Domain.Room.Entities;

public class RoomTypeImage : Entity<RoomTypeImageId>
{
    public RoomTypeId RoomTypeId { get; private set; }
    public string FilePath { get; private set; }
    public bool IsMain { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private RoomTypeImage() : base(RoomTypeImageId.CreateUnique())
    {
        RoomTypeId = null!;
        FilePath = null!;
    }

    public RoomTypeImage(RoomTypeImageId id, RoomTypeId roomTypeId, string filePath) : base(id)
    {
        RoomTypeId = roomTypeId;
        FilePath = filePath;
        IsMain = false;
        UploadedAt = DateTime.UtcNow;
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
