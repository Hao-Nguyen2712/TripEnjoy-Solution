using System.Security.Claims;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Interfaces.Identity
{
    public interface IAuthenService
    {
        Task<Result<(string UserId, string confirmToken)>> CreateUserAsync(string email, string password, string role);
        Task<Result> LoginStepOneAsync(string email, string password);
        Task<Result<(AuthResultDTO AuthResult, string CacheKey)>> LoginStepTwoAsync(string email, string otp);
        Task<Result<string>> ConfirmEmailAsync(string userId, string confirmToken);
        Task<Result<ClaimsPrincipal?>> GetPrincipalFromExpiredToken(string token);
        string GenerateRefreshToken();
        Task<string> GenerateAccessTokenAsync(string userId);
    }
}
