using Hangfire;
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
        private readonly IBackgroundJobClient _backgroundJobClient;

        /// <summary>
        /// Initializes a new instance of AuthenService and stores required dependencies for authentication operations.
        /// </summary>
        public AuthenService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IDistributedCache cache, IEmailService emailService, IBackgroundJobClient backgroundJobClient)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _cache = cache;
            _emailService = emailService;
            _backgroundJobClient = backgroundJobClient;
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

            // Remove old OTP from cache before setting a new one
            await _cache.RemoveAsync(cacheKey);

            var hashOtp = HashingOtpExtension.HashWithSHA256(otp);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, hashOtp, cacheOptions);

            _backgroundJobClient.Enqueue(() => _emailService.SendOtpAsync(email, otp, CancellationToken.None));

            return Result<string>.Success("Sending OTP to email successfully");
        }

        /// <summary>
        /// Verifies a one-time password (OTP) for the given email and, on success, issues access and refresh tokens.
        /// </summary>
        /// <param name="email">The user's email address used as the OTP cache key prefix.</param>
        /// <param name="otp">The plaintext one-time password provided by the user.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a tuple of <see cref="AuthResultDTO"/> and the cache key string on success.
        /// Returns a failure result with InvalidOtp if the cached hashed OTP is missing or does not match the provided OTP,
        /// or NotFound if no user exists for the given email.
        /// </returns>
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

        /// <summary>
        /// Confirms a user's email address using the provided ASP.NET Identity confirmation token.
        /// </summary>
        /// <param name="userId">The Identity user id of the account to confirm.</param>
        /// <param name="confirmToken">The email confirmation token previously generated for the user (e.g. from <c>UserManager.GenerateEmailConfirmationTokenAsync</c>).</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a success message when confirmation succeeds.
        /// On failure returns a Result with:
        /// - a NotFound error if the user id does not exist, or
        /// - one or more mapped Identity errors if confirmation fails.
        /// </returns>
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




        /// <summary>
        /// Authenticate a user using email and password and, on success, issue JWT access and refresh tokens.
        /// </summary>
        /// <remarks>
        /// Uses ASP.NET Identity to validate credentials and enforces lockout and email-confirmation rules via SignInManager.
        /// On success returns an AuthResultDTO with an access token, a refresh token, and the user's ASP.NET Identity id.
        /// On failure returns a failure Result containing one of the domain errors: LoginFailed, LockedOut, EmailNotConfirmed, or TwoFactorRequired.
        /// </remarks>
        /// <returns>
        /// A <see cref="Result{T}"/> whose value on success is a tuple containing:
        /// - <see cref="AuthResultDTO"/>: the issued access and refresh tokens and the user id, and
        /// - <c>string AspNetUserId</c>: the user's Identity id.
        /// On failure the Result contains a corresponding domain error.
        /// </returns>
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

        /// <summary>
        /// Creates a new user with the specified email and password, assigns the given role (creating the role if it does not exist),
        /// and generates an email confirmation token.
        /// </summary>
        /// <param name="role">The role to assign to the new user; the role will be created if it does not already exist.</param>
        /// <returns>
        /// On success, a <see cref="Result{T}"/> containing a tuple with the new user's id and the email confirmation token.
        /// On failure, a failure <see cref="Result{T}"/> with aggregated identity errors describing why creation failed.
        /// </returns>
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

            // Build the confirmation link, now including the role
            var confirmationLink = $"{_configuration["WebAppUrl"]}/confirm-email?userId={user.Id}&token={System.Net.WebUtility.UrlEncode(token)}&confirmFor={role}";

            // Send the confirmation email
            _backgroundJobClient.Enqueue(() => _emailService.SendEmailConfirmationAsync(user.Email, "Confirm Your Email", confirmationLink, CancellationToken.None));

            return Result<(string, string)>.Success((user.Id, token));
        }

        /// <summary>
        /// Creates a short-lived JWT access token and a cryptographically strong refresh token for the specified user.
        /// </summary>
        /// <param name="user">The authenticated ApplicationUser for whom tokens are generated; must have a valid Id and Email.</param>
        /// <returns>
        /// A tuple containing:
        /// - AccessToken: a JWT carrying user id, email, JTI and role claims (expires in ~15 minutes).
        /// - RefreshToken: a securely generated opaque token for renewing access.
        ///
        /// Both values are returned as strings.
        /// </returns>
        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user)
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

        /// <summary>
        /// Generates a cryptographically secure random refresh token encoded as a Base64 string.
        /// </summary>
        /// <returns>A 64-byte cryptographically secure random value encoded in Base64, suitable for use as a refresh token.</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Validates a JWT's signature (without enforcing expiration) and returns the reconstructed <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <remarks>
        /// The method disables lifetime validation so it can extract claims from expired tokens, but still requires a valid signature
        /// using the configured HMAC-SHA256 signing key. If the token is not a valid JWT or does not use HMAC-SHA256, the call fails.
        /// </remarks>
        /// <param name="token">The JWT to validate and extract claims from.</param>
        /// <returns>
        /// A <see cref="Result{ClaimsPrincipal?}"/> containing the principal on success, or a failure result with
        /// <c>Domain.Common.Errors.DomainError.RefreshToken.InvalidToken</c> if the token is malformed, uses a different algorithm,
        /// or fails signature validation.
        /// </returns>
        public Task<Result<ClaimsPrincipal?>> GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(Result<ClaimsPrincipal?>.Failure(Domain.Common.Errors.DomainError.RefreshToken.InvalidToken)); // Token không hợp lệ
            }
            return Task.FromResult(Result<ClaimsPrincipal?>.Success(principal));
        }

        /// <summary>
        /// Generates a new JWT access token for the specified user.
        /// </summary>
        /// <param name="userId">The ASP.NET Identity user id for which to create an access token.</param>
        /// <returns>A freshly generated JWT access token as a string.</returns>
        public async Task<string> GenerateAccessTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var (accessToken, _) = await GenerateTokensAsync(user); // Tái sử dụng hàm đã có
            return accessToken;
        }

        public async Task<Result<string>> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Return a success result even if user not found to prevent email enumeration
                return Result<string>.Success(string.Empty);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Result<string>.Success(token);
        }
    }
}