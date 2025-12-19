using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Payments.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Payments.Handlers;

public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, Result<PaymentStatusDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPaymentStatusQueryHandler> _logger;

    public GetPaymentStatusQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPaymentStatusQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PaymentStatusDto>> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        // Get payment
        var paymentId = PaymentId.Create(request.PaymentId);
        var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId.Value);

        if (payment == null)
        {
            _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
            return Result<PaymentStatusDto>.Failure(DomainError.Payment.NotFound);
        }

        // Map to DTO
        var dto = new PaymentStatusDto
        {
            PaymentId = payment.Id.Value,
            BookingId = payment.BookingId.Id,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(),
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };

        _logger.LogInformation("Successfully retrieved payment status for {PaymentId}", request.PaymentId);

        return Result<PaymentStatusDto>.Success(dto);
    }
}
