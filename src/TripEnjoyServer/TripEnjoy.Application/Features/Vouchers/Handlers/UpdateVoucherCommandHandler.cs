using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Vouchers.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Voucher;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Application.Features.Vouchers.Handlers;

public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateVoucherCommandHandler> _logger;

    public UpdateVoucherCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateVoucherCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucherId = VoucherId.Create(request.VoucherId);

        // Get voucher
        var voucher = await _unitOfWork.Repository<Voucher>()
            .GetAsync(v => v.Id == voucherId);

        if (voucher == null)
        {
            return Result.Failure(DomainError.Voucher.NotFound);
        }

        // Update voucher
        var updateResult = voucher.Update(
            request.Description,
            request.StartDate,
            request.EndDate,
            request.UsageLimit,
            request.UsageLimitPerUser);

        if (updateResult.IsFailure)
        {
            _logger.LogError("Failed to update voucher {VoucherId}: {Errors}",
                voucherId.Value, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return updateResult;
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated voucher {VoucherId}", voucherId.Value);

        return Result.Success();
    }
}
