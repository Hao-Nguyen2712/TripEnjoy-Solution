using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Vouchers.Queries;

public record GetVoucherUsageStatsQuery(Guid VoucherId) : IRequest<Result<VoucherUsageStatsDto>>;

public record VoucherUsageStatsDto(
    Guid VoucherId,
    string Code,
    int UsedCount,
    int? UsageLimit,
    int? UsageLimitPerUser,
    int RemainingUses,
    decimal TotalDiscountGiven
);
