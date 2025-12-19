using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Payments.Commands;

/// <summary>
/// Command to verify payment callback from payment gateway
/// </summary>
public record VerifyPaymentCallbackCommand(
    Dictionary<string, string> CallbackData) : IRequest<Result<bool>>;
