using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Vouchers.Queries;

public record GetVoucherByCodeQuery(string Code) : IRequest<Result<VoucherDto>>;
