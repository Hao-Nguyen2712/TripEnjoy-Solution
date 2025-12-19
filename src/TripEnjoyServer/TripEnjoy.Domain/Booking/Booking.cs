using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.Enums;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Domain.Booking
{
    public class Booking : AggregateRoot<BookingId>
    {
        private readonly List<BookingDetail> _bookingDetails = new();
        private readonly List<BookingHistory> _bookingHistory = new();

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
        public IReadOnlyCollection<BookingDetail> BookingDetails => _bookingDetails.AsReadOnly();
        public IReadOnlyCollection<BookingHistory> BookingHistory => _bookingHistory.AsReadOnly();

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

            // Record initial status in history
            AddHistoryEntry($"Booking created with status {Status}", Status);
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

        public Result AddBookingDetail(BookingDetail bookingDetail)
        {
            _bookingDetails.Add(bookingDetail);
            RecalculateTotalPrice();
            return Result.Success();
        }

        public void RecalculateTotalPrice()
        {
            TotalPrice = _bookingDetails.Sum(bd => bd.TotalPrice);
            UpdatedAt = DateTime.UtcNow;
        }

        private void AddHistoryEntry(string description, string status, UserId? changedBy = null)
        {
            var historyEntry = Entities.BookingHistory.CreateEntry(Id, description, status, changedBy);
            _bookingHistory.Add(historyEntry);
        }

        public Result Confirm(UserId? changedBy = null)
        {
            if (Status != BookingStatusEnum.Pending.ToString())
            {
                return Result.Failure(DomainError.Booking.InvalidStatusTransition);
            }

            Status = BookingStatusEnum.Confirmed.ToString();
            UpdatedAt = DateTime.UtcNow;
            AddHistoryEntry($"Booking confirmed", Status, changedBy);
            return Result.Success();
        }

        public Result Cancel(UserId? changedBy = null)
        {
            if (Status == BookingStatusEnum.Cancelled.ToString() ||
                Status == BookingStatusEnum.Completed.ToString())
            {
                return Result.Failure(DomainError.Booking.CannotCancelBooking);
            }

            Status = BookingStatusEnum.Cancelled.ToString();
            UpdatedAt = DateTime.UtcNow;
            AddHistoryEntry($"Booking cancelled", Status, changedBy);
            return Result.Success();
        }

        public Result CheckIn(UserId? changedBy = null)
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
            AddHistoryEntry($"Guest checked in", Status, changedBy);
            return Result.Success();
        }

        public Result CheckOut(UserId? changedBy = null)
        {
            if (Status != BookingStatusEnum.CheckedIn.ToString())
            {
                return Result.Failure(DomainError.Booking.InvalidStatusTransition);
            }

            Status = BookingStatusEnum.CheckedOut.ToString();
            UpdatedAt = DateTime.UtcNow;
            AddHistoryEntry($"Guest checked out", Status, changedBy);
            return Result.Success();
        }

        public Result Complete(UserId? changedBy = null)
        {
            if (Status != BookingStatusEnum.CheckedOut.ToString())
            {
                return Result.Failure(DomainError.Booking.InvalidStatusTransition);
            }

            Status = BookingStatusEnum.Completed.ToString();
            UpdatedAt = DateTime.UtcNow;
            AddHistoryEntry($"Booking completed", Status, changedBy);
            return Result.Success();
        }

        public int CalculateNights()
        {
            return (CheckOutDate - CheckInDate).Days;
        }
    }
}
