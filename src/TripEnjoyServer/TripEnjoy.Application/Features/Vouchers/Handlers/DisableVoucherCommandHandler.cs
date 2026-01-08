using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Vouchers.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Voucher;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Application.Features.Vouchers.Handlers;

public class DisableVoucherCommandHandler : IRequestHandler<DisableVoucherCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DisableVoucherCommandHandler> _logger;

    public DisableVoucherCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DisableVoucherCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DisableVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucherId = VoucherId.Create(request.VoucherId);

        // Get voucher
        var voucher = await _unitOfWork.Repository<Voucher>()
            .GetAsync(v => v.Id == voucherId);

        if (voucher == null)
        {
            return Result.Failure(DomainError.Voucher.NotFound);
        }

        // Disable voucher
        var disableResult = voucher.Disable();

        if (disableResult.IsFailure)
        {
            _logger.LogError("Failed to disable voucher {VoucherId}: {Errors}",
                voucherId.Value, string.Join(", ", disableResult.Errors.Select(e => e.Description)));
            return disableResult;
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully disabled voucher {VoucherId}", voucherId.Value);

        return Result.Success();
    }
}
