using MediatR;
using TripEnjoy.Application.Features.Admin.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class GetAllVouchersQueryHandler : IRequestHandler<GetAllVouchersQuery, Result<IEnumerable<VoucherDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllVouchersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<VoucherDto>>> Handle(GetAllVouchersQuery request, CancellationToken cancellationToken)
    {
        var vouchers = await _unitOfWork.Repository<Domain.Voucher.Voucher>().GetAllAsync();

        var voucherDtos = vouchers.Select(v => new VoucherDto
        {
            Id = v.Id.Value,
            Code = v.Code,
            Description = v.Description,
            DiscountType = v.DiscountType,
            DiscountValue = v.DiscountValue,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            UsageLimit = v.UsageLimit,
            UsedCount = v.UsedCount,
            UsageLimitPerUser = v.UsageLimitPerUser,
            Status = v.Status,
            CreatedAt = v.CreatedAt
        }).OrderByDescending(v => v.CreatedAt);

        return Result<IEnumerable<VoucherDto>>.Success(voucherDtos);
    }
}
