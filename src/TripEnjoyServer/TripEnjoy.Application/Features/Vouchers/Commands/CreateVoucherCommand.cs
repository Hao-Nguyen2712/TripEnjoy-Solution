using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Voucher.Enums;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Application.Features.Vouchers.Commands;

public record CreateVoucherCommand(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    string? Description = null,
    decimal? MinimumOrderAmount = null,
    decimal? MaximumDiscountAmount = null,
    int? UsageLimit = null,
    int? UsageLimitPerUser = null,
    List<VoucherTargetDto>? Targets = null
) : IAuditableCommand<Result<VoucherId>>;

public record VoucherTargetDto(
    string TargetType,
    Guid? PartnerId = null,
    Guid? PropertyId = null,
    Guid? RoomTypeId = null
);
