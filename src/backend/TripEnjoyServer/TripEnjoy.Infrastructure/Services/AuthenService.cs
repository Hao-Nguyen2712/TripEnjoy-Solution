using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.ShareKernel.Dtos;
using TripEnjoy.Domain.Account.Entities;

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
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return Result.Failure(Domain.Common.Errors.DomainError.Account.LoginFailed);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Result.Failure(Domain.Common.Errors.DomainError.Account.EmailNotConfirmed);
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"otp:{email}";
            
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            };
            await _cache.SetStringAsync(cacheKey, otp, cacheOptions);

            await _emailService.SendOtpAsync(email, otp);

            return Result.Success();
        }

        public async Task<Result<AuthResultDTO>> LoginStepTwoAsync(string email, string otp)
        {
            var cacheKey = $"otp:{email}";
            var storedOtp = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(storedOtp) || storedOtp != otp)
            {
                return Result<AuthResultDTO>.Failure(Domain.Common.Errors.DomainError.Account.InvalidOtp);
            }
            
            await _cache.RemoveAsync(cacheKey);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<AuthResultDTO>.Failure(Domain.Common.Errors.DomainError.User.NotFound);
            }
            
            // Reusing token generation logic
            var (accessToken, refreshTokenString) = await GenerateTokensAsync(user);
            var authResult = new AuthResultDTO(accessToken, refreshTokenString, user.Id);

            return Result<AuthResultDTO>.Success(authResult);
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

            return Result<string>.Success("Create account successfully");
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