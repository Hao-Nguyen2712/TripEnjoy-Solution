using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Vouchers.Commands;

public record DisableVoucherCommand(Guid VoucherId) : IAuditableCommand<Result>;
