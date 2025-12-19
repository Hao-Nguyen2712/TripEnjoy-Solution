using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Domain.Booking.Entities;

/// <summary>
/// Represents a single room type booking within a booking (multi-room support)
/// </summary>
public class BookingDetail : Entity<BookingDetailId>
{
    public BookingId BookingId { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public int Quantity { get; private set; }
    public int Nights { get; private set; }
    public decimal PricePerNight { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalPrice { get; private set; }

    // Navigation property
    public Booking Booking { get; private set; } = null!;

    private BookingDetail() : base(BookingDetailId.CreateUnique())
    {
        BookingId = null!;
        RoomTypeId = null!;
    }

    private BookingDetail(
        BookingDetailId id,
        BookingId bookingId,
        RoomTypeId roomTypeId,
        int quantity,
        int nights,
        decimal pricePerNight,
        decimal discountAmount) : base(id)
    {
        BookingId = bookingId;
        RoomTypeId = roomTypeId;
        Quantity = quantity;
        Nights = nights;
        PricePerNight = pricePerNight;
        DiscountAmount = discountAmount;
        TotalPrice = CalculateTotalPrice(quantity, nights, pricePerNight, discountAmount);
    }

    public static Result<BookingDetail> Create(
        BookingId bookingId,
        RoomTypeId roomTypeId,
        int quantity,
        int nights,
        decimal pricePerNight,
        decimal discountAmount = 0)
    {
        // Validations
        if (quantity <= 0)
        {
            return Result<BookingDetail>.Failure(DomainError.Booking.InvalidRoomQuantity);
        }

        if (nights <= 0)
        {
            return Result<BookingDetail>.Failure(DomainError.Booking.InvalidNights);
        }

        if (pricePerNight < 0)
        {
            return Result<BookingDetail>.Failure(DomainError.Booking.InvalidPricePerNight);
        }

        if (discountAmount < 0)
        {
            return Result<BookingDetail>.Failure(DomainError.Booking.InvalidDiscountAmount);
        }

        var totalPrice = CalculateTotalPrice(quantity, nights, pricePerNight, discountAmount);
        if (totalPrice < 0)
        {
            return Result<BookingDetail>.Failure(DomainError.Booking.InvalidTotalPrice);
        }

        var bookingDetail = new BookingDetail(
            BookingDetailId.CreateUnique(),
            bookingId,
            roomTypeId,
            quantity,
            nights,
            pricePerNight,
            discountAmount);

        return Result<BookingDetail>.Success(bookingDetail);
    }

    private static decimal CalculateTotalPrice(int quantity, int nights, decimal pricePerNight, decimal discountAmount)
    {
        return (pricePerNight * quantity * nights) - discountAmount;
    }

    public Result UpdateDiscount(decimal newDiscountAmount)
    {
        if (newDiscountAmount < 0)
        {
            return Result.Failure(DomainError.Booking.InvalidDiscountAmount);
        }

        DiscountAmount = newDiscountAmount;
        TotalPrice = CalculateTotalPrice(Quantity, Nights, PricePerNight, newDiscountAmount);

        if (TotalPrice < 0)
        {
            return Result.Failure(DomainError.Booking.InvalidTotalPrice);
        }

        return Result.Success();
    }
}
