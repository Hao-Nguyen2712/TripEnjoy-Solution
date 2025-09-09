using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.ShareKernel.Dtos;
using TripEnjoy.ShareKernel.Extensions;

namespace TripEnjoy.Infrastructure.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly IEmailService _emailService;

        public AuthenService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IDistributedCache cache, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<Result> LoginStepOneAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result.Failure(Domain.Common.Errors.DomainError.Account.LoginFailed);
            }
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);

            if (!passwordValid)
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(5);
                    user.AccessFailedCount = 0; // Reset after lockout
                }
                await _userManager.UpdateAsync(user);
                return Result.Failure(Domain.Common.Errors.DomainError.Account.LoginFailed);
            }
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                return Result.Failure(Domain.Common.Errors.DomainError.Account.LockedOut);
            }


            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Result.Failure(Domain.Common.Errors.DomainError.Account.EmailNotConfirmed);
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"otp:{email}";
            var hashOtp = HashingOtpExtension.HashWithSHA256(otp);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, hashOtp, cacheOptions);

            await _emailService.SendOtpAsync(email, otp);

            return Result<string>.Success("Sending OTP to email successfully");
        }

        public async Task<Result<(AuthResultDTO AuthResult, string CacheKey)>> LoginStepTwoAsync(string email, string otp)
        {
            var cacheKey = $"otp:{email}";
            var storedOtp = await _cache.GetStringAsync(cacheKey);
            var hashOtp = HashingOtpExtension.HashWithSHA256(otp);

            if (string.IsNullOrEmpty(storedOtp) || storedOtp != hashOtp)
            {
                return Result<(AuthResultDTO, string)>.Failure(Domain.Common.Errors.DomainError.Account.InvalidOtp);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<(AuthResultDTO, string)>.Failure(Domain.Common.Errors.DomainError.User.NotFound);
            }

            // Reusing token generation logic
            var (accessToken, refreshTokenString) = await GenerateTokensAsync(user);
            var authResult = new AuthResultDTO(accessToken, refreshTokenString, user.Id);

            return Result<(AuthResultDTO, string)>.Success((authResult, cacheKey));
        }

        public async Task<Result<string>> ConfirmEmailAsync(string userId, string confirmToken)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<string>.Failure(new Error("User.NotFound", "User not found.", Domain.Common.Errors.ErrorType.NotFound));
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmToken);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(e.Code, e.Description, Domain.Common.Errors.ErrorType.Failure)).ToArray();
                return Result<string>.Failure(errors);
            }

            return Result<string>.Success("Confirm Email SuccessFully");
        }




        public async Task<Result<(AuthResultDTO AuthResult, string AspNetUserId)>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<(AuthResultDTO, string)>.Failure(Domain.Common.Errors.DomainError.Account.LoginFailed);
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return Result<(AuthResultDTO, string)>.Failure(Domain.Common.Errors.DomainError.Account.LockedOut);
            }
            if (result.IsNotAllowed)
            {
                return Result<(AuthResultDTO, string)>.Failure(Domain.Common.Errors.DomainError.Account.EmailNotConfirmed);
            }
            if (result.RequiresTwoFactor)
            {
                return Result<(AuthResultDTO, string)>.Failure(Domain.Common.Errors.DomainError.Account.TwoFactorRequired);
            }
            if (!result.Succeeded)
            {
                return Result<(AuthResultDTO, string)>.Failure(Domain.Common.Errors.DomainError.Account.LoginFailed);
            }


            var authClaims = new List<Claim>
             {
                 new Claim(ClaimTypes.NameIdentifier, user.Id),
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             };


            var userRole = await _userManager.GetRolesAsync(user);
            foreach (var roles in userRole)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
            }
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            var authResult = new AuthResultDTO(accessToken, refreshToken, user.Id);

            return Result<(AuthResultDTO, string)>.Success((authResult, user.Id));
        }

        public async Task<Result<(string UserId, string confirmToken)>> CreateUserAsync(string email, string password, string role)
        {
            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
            };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Domain.Common.Errors.Error(e.Code, e.Description, Domain.Common.Errors.ErrorType.Failure)).ToArray();
                return Result<(string, string)>.Failure(errors);
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return Result<(string, string)>.Success((user.Id, token));
        }

        private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(15),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            return (accessToken, refreshToken);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}