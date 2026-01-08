using System.ComponentModel.DataAnnotations;

namespace TripEnjoy.Client.Models;

public class CreateBookingRequest
{
    [Required]
    public Guid PropertyId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    [Range(1, 100)]
    public int NumberOfGuests { get; set; }

    public string? SpecialRequests { get; set; }

    [Required]
    public List<BookingRoomItem> Rooms { get; set; } = new();
}

public class BookingRoomItem
{
    [Required]
    public Guid RoomTypeId { get; set; }

    [Required]
    [Range(1, 10)]
    public int Quantity { get; set; }

    public Guid? VoucherCode { get; set; }
}

public class ProcessPaymentRequest
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    public string ReturnUrl { get; set; } = string.Empty;
}

public class CreateReviewRequest
{
    [Required]
    public Guid BookingDetailId { get; set; }

    [Required]
    public Guid RoomTypeId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Comment { get; set; } = string.Empty;

    public List<string>? ImageUrls { get; set; }
}

public class UpdateReviewRequest
{
    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Comment { get; set; } = string.Empty;
}

public class CreateReplyRequest
{
    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Content { get; set; } = string.Empty;
}

public class UpdateReplyRequest
{
    [Required]
    public Guid ReviewReplyId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Content { get; set; } = string.Empty;
}
