using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Commands;

public record RejectPartnerCommand(Guid PartnerId, string Reason) : IRequest<Result>;
