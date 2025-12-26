using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Vouchers.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Voucher;
using TripEnjoy.Domain.Voucher.Enums;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Vouchers.Handlers;

public class GetActiveVouchersQueryHandler : IRequestHandler<GetActiveVouchersQuery, Result<List<VoucherDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetActiveVouchersQueryHandler> _logger;

    public GetActiveVouchersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetActiveVouchersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<VoucherDto>>> Handle(GetActiveVouchersQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Get all vouchers and filter in memory
        var allVouchers = await _unitOfWork.Repository<Voucher>().GetAllAsync();
        var vouchers = allVouchers
            .Where(v => v.Status == VoucherStatusEnum.Active.ToString() &&
                       v.StartDate <= now &&
                       v.EndDate >= now)
            .OrderByDescending(v => v.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var voucherDtos = vouchers.Select(v => new VoucherDto
        {
            Id = v.Id.Value,
            Code = v.Code,
            Description = v.Description,
            DiscountType = v.DiscountType,
            DiscountValue = v.DiscountValue,
            MinimumOrderAmount = v.MinimumOrderAmount,
            MaximumDiscountAmount = v.MaximumDiscountAmount,
            UsageLimit = v.UsageLimit,
            UsageLimitPerUser = v.UsageLimitPerUser,
            UsedCount = v.UsedCount,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            Status = v.Status,
            CreatorType = v.CreatorType,
            CreatorId = v.CreatorId.Id,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt,
            Targets = v.VoucherTargets.Select(t => new VoucherTargetDto
            {
                Id = t.Id.Value,
                TargetType = t.TargetType,
                PartnerId = t.PartnerId?.Id,
                PropertyId = t.PropertyId?.Id,
                RoomTypeId = t.RoomTypeId?.Value
            }).ToList()
        }).ToList();

        _logger.LogInformation("Retrieved {Count} active vouchers", voucherDtos.Count);

        return Result<List<VoucherDto>>.Success(voucherDtos);
    }
}
