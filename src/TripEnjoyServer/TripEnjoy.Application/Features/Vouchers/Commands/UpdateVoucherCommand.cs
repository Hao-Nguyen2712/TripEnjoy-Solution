using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Vouchers.Commands;

public record UpdateVoucherCommand(
    Guid VoucherId,
    string? Description = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? UsageLimit = null,
    int? UsageLimitPerUser = null
) : IAuditableCommand<Result>;
