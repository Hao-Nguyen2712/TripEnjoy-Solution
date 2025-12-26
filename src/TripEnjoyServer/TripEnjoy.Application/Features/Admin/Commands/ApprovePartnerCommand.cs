using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Commands;

public record ApprovePartnerCommand(Guid PartnerId) : IRequest<Result>;
