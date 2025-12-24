using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Reviews.Commands;

public record HideReviewCommand(Guid ReviewId, bool Hide = true) : IAuditableCommand<Result>;
