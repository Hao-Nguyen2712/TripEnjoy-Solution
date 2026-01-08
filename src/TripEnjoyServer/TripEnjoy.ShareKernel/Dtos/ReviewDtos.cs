namespace TripEnjoy.ShareKernel.Dtos;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid BookingDetailId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ReviewImageDto> Images { get; set; } = new();
    public List<ReviewReplyDto> Replies { get; set; } = new();
}

public class ReviewImageDto
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class ReviewReplyDto
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public string ReplierType { get; set; } = string.Empty;
    public Guid ReplierId { get; set; }
    public string ReplierName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReviewSummaryDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ImageCount { get; set; }
    public int ReplyCount { get; set; }
}
