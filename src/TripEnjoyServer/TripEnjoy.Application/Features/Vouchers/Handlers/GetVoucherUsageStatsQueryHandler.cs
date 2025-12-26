using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Vouchers.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Voucher;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Application.Features.Vouchers.Handlers;

public class GetVoucherUsageStatsQueryHandler : IRequestHandler<GetVoucherUsageStatsQuery, Result<VoucherUsageStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetVoucherUsageStatsQueryHandler> _logger;

    public GetVoucherUsageStatsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetVoucherUsageStatsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<VoucherUsageStatsDto>> Handle(GetVoucherUsageStatsQuery request, CancellationToken cancellationToken)
    {
        var voucherId = VoucherId.Create(request.VoucherId);

        var vouchers = await _unitOfWork.Repository<Voucher>().GetAllAsync();
        var voucher = vouchers.FirstOrDefault(v => v.Id == voucherId);

        if (voucher == null)
        {
            return Result<VoucherUsageStatsDto>.Failure(DomainError.Voucher.NotFound);
        }

        // Calculate total discount given
        // Note: This requires querying BookingVoucher entities which don't exist yet
        // TODO: Implement BookingVoucher entity and uncomment this logic
        decimal totalDiscountGiven = 0;

        /* 
        try
        {
            // Try to calculate if BookingVoucher entities exist
            var allBookingVouchers = await _unitOfWork.Repository<BookingVoucher>().GetAllAsync();
            var bookingVouchers = allBookingVouchers.Where(bv => bv.VoucherId == voucherId).ToList();

            totalDiscountGiven = bookingVouchers.Sum(bv => bv.DiscountAmount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not calculate total discount given for voucher {VoucherId}", voucherId.Value);
        }
        */

        int remainingUses = voucher.UsageLimit.HasValue 
            ? Math.Max(0, voucher.UsageLimit.Value - voucher.UsedCount)
            : int.MaxValue;

        var stats = new VoucherUsageStatsDto(
            voucher.Id.Value,
            voucher.Code,
            voucher.UsedCount,
            voucher.UsageLimit,
            voucher.UsageLimitPerUser,
            remainingUses,
            totalDiscountGiven);

        return Result<VoucherUsageStatsDto>.Success(stats);
    }
}
