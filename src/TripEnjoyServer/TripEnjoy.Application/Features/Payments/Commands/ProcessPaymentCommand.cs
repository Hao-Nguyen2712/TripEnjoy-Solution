using MediatR;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Payments.Commands;

/// <summary>
/// Command to process a payment for a booking
/// </summary>
public record ProcessPaymentCommand(
    Guid BookingId,
    string PaymentMethod,
    string ReturnUrl) : IRequest<Result<string>>;
