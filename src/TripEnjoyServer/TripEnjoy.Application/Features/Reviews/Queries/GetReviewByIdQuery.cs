using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Reviews.Queries;

public record GetReviewByIdQuery(Guid ReviewId) : IRequest<Result<ReviewDto>>;
