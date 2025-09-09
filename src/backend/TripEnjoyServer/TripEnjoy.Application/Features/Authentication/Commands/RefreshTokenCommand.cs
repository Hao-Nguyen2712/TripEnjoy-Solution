using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record RefreshTokenCommand
    (
        string expiredAccessToken,
        string refreshToken
    ) : IRequest<Result<AuthResultDTO>>;

}
