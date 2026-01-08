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
using TripEnjoy.Domain.Voucher.Entities;
using TripEnjoy.Domain.Voucher.Enums;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Application.Features.Vouchers.Handlers;

public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, Result<VoucherId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateVoucherCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateVoucherCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateVoucherCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<VoucherId>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        // Get user ID and role from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result<VoucherId>.Failure(new Error(
                "Voucher.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var creatorId = AccountId.Create(userIdGuid);

        // Determine creator type based on role
        var creatorType = userRole?.ToLower() switch
        {
            "admin" => VoucherCreatorTypeEnum.Admin,
            "partner" => VoucherCreatorTypeEnum.Partner,
            _ => VoucherCreatorTypeEnum.Partner // Default to Partner
        };

        // Only admins can create global vouchers
        if (creatorType != VoucherCreatorTypeEnum.Admin && 
            (request.Targets == null || !request.Targets.Any()))
        {
            return Result<VoucherId>.Failure(new Error(
                "Voucher.UnauthorizedGlobalCreation",
                "Only administrators can create global vouchers.",
                ErrorType.Unauthorized));
        }

        // Parse discount type enum
        if (!Enum.TryParse<VoucherDiscountTypeEnum>(request.DiscountType, out var discountType))
        {
            return Result<VoucherId>.Failure(new Error(
                "Voucher.InvalidDiscountType",
                "Invalid discount type provided.",
                ErrorType.Validation));
        }

        // Check if voucher code already exists
        var existingVoucher = await _unitOfWork.Repository<Voucher>()
            .GetAsync(v => v.Code == request.Code.ToUpperInvariant());

        if (existingVoucher != null)
        {
            return Result<VoucherId>.Failure(DomainError.Voucher.DuplicateCode);
        }

        // Create voucher
        var voucherResult = Voucher.Create(
            request.Code,
            discountType,
            request.DiscountValue,
            request.StartDate,
            request.EndDate,
            creatorType,
            creatorId,
            request.Description,
            request.MinimumOrderAmount,
            request.MaximumDiscountAmount,
            request.UsageLimit,
            request.UsageLimitPerUser);

        if (voucherResult.IsFailure)
        {
            _logger.LogError("Failed to create voucher: {Errors}",
                string.Join(", ", voucherResult.Errors.Select(e => e.Description)));
            return Result<VoucherId>.Failure(voucherResult.Errors);
        }

        var voucher = voucherResult.Value;

        // Add targets if provided
        if (request.Targets != null && request.Targets.Any())
        {
            foreach (var targetDto in request.Targets)
            {
                if (!Enum.TryParse<VoucherTargetTypeEnum>(targetDto.TargetType, out var targetType))
                {
                    continue; // Skip invalid target types (already validated)
                }

                VoucherTarget target = targetType switch
                {
                    VoucherTargetTypeEnum.Global => VoucherTarget.CreateGlobal(voucher.Id),
                    VoucherTargetTypeEnum.Partner when targetDto.PartnerId.HasValue => 
                        VoucherTarget.CreateForPartner(voucher.Id, AccountId.Create(targetDto.PartnerId.Value)),
                    VoucherTargetTypeEnum.Property when targetDto.PropertyId.HasValue => 
                        VoucherTarget.CreateForProperty(voucher.Id, PropertyId.Create(targetDto.PropertyId.Value)),
                    VoucherTargetTypeEnum.RoomType when targetDto.RoomTypeId.HasValue => 
                        VoucherTarget.CreateForRoomType(voucher.Id, RoomTypeId.Create(targetDto.RoomTypeId.Value)),
                    _ => null!
                };

                if (target != null)
                {
                    voucher.AddTarget(target);
                }
            }
        }
        else if (creatorType == VoucherCreatorTypeEnum.Admin)
        {
            // Admin creating voucher without targets = Global voucher
            voucher.AddTarget(VoucherTarget.CreateGlobal(voucher.Id));
        }

        // Save to database
        await _unitOfWork.Repository<Voucher>().AddAsync(voucher);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created voucher {VoucherId} with code {Code} by {CreatorType} {CreatorId}",
            voucher.Id.Value, voucher.Code, creatorType, creatorId.Id);

        return Result<VoucherId>.Success(voucher.Id);
    }
}
