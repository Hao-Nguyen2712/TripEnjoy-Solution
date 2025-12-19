using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.Enums;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Domain.Booking
{
    public class Booking : AggregateRoot<BookingId>
    {
        public UserId UserId { get; private set; }
        public PropertyId PropertyId { get; private set; }
        public DateTime CheckInDate { get; private set; }
        public DateTime CheckOutDate { get; private set; }
        public int NumberOfGuests { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? SpecialRequests { get; private set; }

        // Navigation properties
        public Domain.Account.Entities.User User { get; private set; }
        public Domain.Property.Property Property { get; private set; }

        private Booking() : base(BookingId.CreateUnique())
        {
            UserId = null!;
            PropertyId = null!;
            User = null!;
            Property = null!;
            Status = null!;
        }

        public Booking(
            BookingId id,
            UserId userId,
            PropertyId propertyId,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests,
            decimal totalPrice,
            string? specialRequests = null) : base(id)
        {
            UserId = userId;
            PropertyId = propertyId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfGuests = numberOfGuests;
            TotalPrice = totalPrice;
            SpecialRequests = specialRequests;
            Status = BookingStatusEnum.Pending.ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
        }

        public static Result<Booking> Create(
            UserId userId,
            PropertyId propertyId,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests,
            decimal totalPrice,
            string? specialRequests = null)
        {
            // Validation
            if (checkInDate < DateTime.UtcNow.Date)
            {
                return Result<Booking>.Failure(DomainError.Booking.InvalidCheckInDate);
            }

            if (checkOutDate <= checkInDate)
            {
                return Result<Booking>.Failure(DomainError.Booking.InvalidCheckOutDate);
            }

            if (numberOfGuests <= 0)
            {
                return Result<Booking>.Failure(DomainError.Booking.InvalidGuestCount);
            }

            if (totalPrice < 0)
            {
                return Result<Booking>.Failure(DomainError.Booking.InvalidTotalPrice);
            }

            var booking = new Booking(
                BookingId.CreateUnique(),
                userId,
                propertyId,
                checkInDate,
                checkOutDate,
                numberOfGuests,
                totalPrice,
                specialRequests);

            return Result<Booking>.Success(booking);
        }

        public Result Confirm()
        {
            if (Status != BookingStatusEnum.Pending.ToString())
            {
                return Result.Failure(DomainError.Booking.InvalidStatusTransition);
            }

            Status = BookingStatusEnum.Confirmed.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Cancel()
        {
            if (Status == BookingStatusEnum.Cancelled.ToString() ||
                Status == BookingStatusEnum.Completed.ToString())
            {
                return Result.Failure(DomainError.Booking.CannotCancelBooking);
            }

            Status = BookingStatusEnum.Cancelled.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result CheckIn()
        {
            if (Status != BookingStatusEnum.Confirmed.ToString())
            {
                return Result.Failure(DomainError.Booking.InvalidStatusTransition);
            }

            if (DateTime.UtcNow.Date < CheckInDate.Date)
            {
                return Result.Failure(DomainError.Booking.CheckInTooEarly);
            }

            Status = BookingStatusEnum.CheckedIn.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result CheckOut()
        {
            if (Status != BookingStatusEnum.CheckedIn.ToString())
            {
                return Result.Failure(DomainError.Booking.InvalidStatusTransition);
            }

            Status = BookingStatusEnum.CheckedOut.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Complete()
        {
            if (Status != BookingStatusEnum.CheckedOut.ToString())
            {
                return Result.Failure(DomainError.Booking.InvalidStatusTransition);
            }

            Status = BookingStatusEnum.Completed.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
