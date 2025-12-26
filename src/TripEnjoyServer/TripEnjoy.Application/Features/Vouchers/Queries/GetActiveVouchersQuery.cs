using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Vouchers.Queries;

public record GetActiveVouchersQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<List<VoucherDto>>>;
