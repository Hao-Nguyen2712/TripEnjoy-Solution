using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Payments.Commands;

/// <summary>
/// Command to refund a payment
/// </summary>
public record RefundPaymentCommand(
    Guid PaymentId,
    string Reason) : IRequest<Result<string>>;
