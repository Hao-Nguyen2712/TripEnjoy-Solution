using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Vouchers.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;
using TripEnjoy.Domain.Voucher;
using TripEnjoy.Domain.Voucher.Enums;

namespace TripEnjoy.Application.Features.Vouchers.Handlers;

public class ApplyVoucherCommandHandler : IRequestHandler<ApplyVoucherCommand, Result<VoucherApplicationResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApplyVoucherCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplyVoucherCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ApplyVoucherCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<VoucherApplicationResult>> Handle(ApplyVoucherCommand request, CancellationToken cancellationToken)
    {
        // Get user ID from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result<VoucherApplicationResult>.Failure(new Error(
                "Voucher.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var userId = UserId.Create(userIdGuid);

        // Find voucher by code
        var allVouchers = await _unitOfWork.Repository<Voucher>().GetAllAsync();
        var voucher = allVouchers.FirstOrDefault(v => v.Code == request.Code.ToUpperInvariant());

        if (voucher == null)
        {
            return Result<VoucherApplicationResult>.Failure(DomainError.Voucher.NotFound);
        }

        // Validate voucher can be used
        var validateResult = voucher.ValidateForUse();
        if (validateResult.IsFailure)
        {
            _logger.LogWarning("Voucher {Code} validation failed: {Errors}",
                request.Code, string.Join(", ", validateResult.Errors.Select(e => e.Description)));
            return Result<VoucherApplicationResult>.Failure(validateResult.Errors);
        }

        // Check minimum order amount
        if (voucher.MinimumOrderAmount.HasValue && request.OrderAmount < voucher.MinimumOrderAmount.Value)
        {
            return Result<VoucherApplicationResult>.Failure(new Error(
                "Voucher.MinimumOrderAmountNotMet",
                $"Order amount must be at least {voucher.MinimumOrderAmount.Value:C}.",
                ErrorType.Validation));
        }

        // Check per-user usage limit
        if (voucher.UsageLimitPerUser.HasValue)
        {
            // TODO: Check BookingVoucher table for user's usage count
            // This would require querying the Booking aggregate's BookingVoucher entities
            // For now, we'll skip this check as BookingVoucher relationship isn't directly accessible
        }

        // Validate target applicability
        if (voucher.VoucherTargets.Any())
        {
            bool isApplicable = false;

            foreach (var target in voucher.VoucherTargets)
            {
                if (target.TargetType == VoucherTargetTypeEnum.Global.ToString())
                {
                    isApplicable = true;
                    break;
                }

                if (target.TargetType == VoucherTargetTypeEnum.Property.ToString() && 
                    request.PropertyId.HasValue &&
                    target.PropertyId?.Id == request.PropertyId.Value)
                {
                    isApplicable = true;
                    break;
                }

                if (target.TargetType == VoucherTargetTypeEnum.RoomType.ToString() && 
                    request.RoomTypeId.HasValue &&
                    target.RoomTypeId?.Value == request.RoomTypeId.Value)
                {
                    isApplicable = true;
                    break;
                }

                // For Partner target type, would need to check if property belongs to partner
                if (target.TargetType == VoucherTargetTypeEnum.Partner.ToString() && 
                    request.PropertyId.HasValue)
                {
                    // Check if property belongs to the target partner
                    var property = await _unitOfWork.Repository<Domain.Property.Property>()
                        .GetByIdAsync(request.PropertyId.Value);

                    if (property != null && property.PartnerId?.Id == target.PartnerId?.Id)
                    {
                        isApplicable = true;
                        break;
                    }
                }
            }

            if (!isApplicable)
            {
                return Result<VoucherApplicationResult>.Failure(new Error(
                    "Voucher.NotApplicable",
                    "This voucher is not applicable to the selected items.",
                    ErrorType.Validation));
            }
        }

        // Calculate discount
        var discountAmount = voucher.CalculateDiscount(request.OrderAmount);
        var finalAmount = request.OrderAmount - discountAmount;

        _logger.LogInformation("Voucher {Code} applied successfully for user {UserId}. Discount: {Discount}, Final: {Final}",
            voucher.Code, userId.Id, discountAmount, finalAmount);

        var result = new VoucherApplicationResult(
            voucher.Id.Value,
            voucher.Code,
            discountAmount,
            finalAmount);

        return Result<VoucherApplicationResult>.Success(result);
    }
}
