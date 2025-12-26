using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Vouchers.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Voucher;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Vouchers.Handlers;

public class GetVoucherByCodeQueryHandler : IRequestHandler<GetVoucherByCodeQuery, Result<VoucherDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetVoucherByCodeQueryHandler> _logger;

    public GetVoucherByCodeQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetVoucherByCodeQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<VoucherDto>> Handle(GetVoucherByCodeQuery request, CancellationToken cancellationToken)
    {
        // Get all vouchers and find by code
        var vouchers = await _unitOfWork.Repository<Voucher>().GetAllAsync();
        var voucher = vouchers.FirstOrDefault(v => v.Code == request.Code.ToUpperInvariant());

        if (voucher == null)
        {
            return Result<VoucherDto>.Failure(DomainError.Voucher.NotFound);
        }

        var voucherDto = new VoucherDto
        {
            Id = voucher.Id.Value,
            Code = voucher.Code,
            Description = voucher.Description,
            DiscountType = voucher.DiscountType,
            DiscountValue = voucher.DiscountValue,
            MinimumOrderAmount = voucher.MinimumOrderAmount,
            MaximumDiscountAmount = voucher.MaximumDiscountAmount,
            UsageLimit = voucher.UsageLimit,
            UsageLimitPerUser = voucher.UsageLimitPerUser,
            UsedCount = voucher.UsedCount,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            Status = voucher.Status,
            CreatorType = voucher.CreatorType,
            CreatorId = voucher.CreatorId.Id,
            CreatedAt = voucher.CreatedAt,
            UpdatedAt = voucher.UpdatedAt,
            Targets = voucher.VoucherTargets.Select(t => new VoucherTargetDto
            {
                Id = t.Id.Value,
                TargetType = t.TargetType,
                PartnerId = t.PartnerId?.Id,
                PropertyId = t.PropertyId?.Id,
                RoomTypeId = t.RoomTypeId?.Value
            }).ToList()
        };

        return Result<VoucherDto>.Success(voucherDto);
    }
}
