using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Vouchers.Commands;

public record ApplyVoucherCommand(
    string Code,
    decimal OrderAmount,
    Guid? PropertyId = null,
    Guid? RoomTypeId = null
) : IAuditableCommand<Result<VoucherApplicationResult>>;

public record VoucherApplicationResult(
    Guid VoucherId,
    string Code,
    decimal DiscountAmount,
    decimal FinalAmount
);
