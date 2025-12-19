using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Payments.Queries;

/// <summary>
/// Query to get payment status by payment ID
/// </summary>
public record GetPaymentStatusQuery(Guid PaymentId) : IRequest<Result<PaymentStatusDto>>;
