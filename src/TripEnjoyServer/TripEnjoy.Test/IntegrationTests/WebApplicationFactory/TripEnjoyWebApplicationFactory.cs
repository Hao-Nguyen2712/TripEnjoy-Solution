using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.PropertyType;
using TripEnjoy.ShareKernel.Dtos;
using TripEnjoy.Infrastructure.Persistence.Seeding;
using System.Security.Claims;

namespace TripEnjoy.Test.IntegrationTests.WebApplicationFactory;

public class TripEnjoyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test environment first  
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TripEnjoyDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add InMemory database for testing - use shared database name
            var databaseName = "TripEnjoyTestDb";
            services.AddDbContext<TripEnjoyDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
            });

            // Replace external services with test doubles (as Singletons to maintain state across requests)
            services.Replace(ServiceDescriptor.Singleton<IEmailService, TestEmailService>());
            services.Replace(ServiceDescriptor.Singleton<ICacheService, TestCacheService>());
            services.Replace(ServiceDescriptor.Singleton<IAuthenService, TestAuthenService>());
        });
        




        // Configure the startup behavior to avoid issues
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Ensure configuration for testing
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("JWT:Secret", "test-secret-key-that-is-long-enough-for-jwt-token-generation"),
                new KeyValuePair<string, string?>("JWT:ValidIssuer", "TestIssuer"),
                new KeyValuePair<string, string?>("JWT:ValidAudience", "TestAudience"),
                new KeyValuePair<string, string?>("CacheSettings:ConnectionString", "localhost:6379")
            });
        });
    }




}

#region Test Service Implementations

public class TestEmailService : IEmailService
{
    public Task<Result> SendEmailConfirmationAsync(string userEmail, string userId, string token, CancellationToken ct)
        => Task.FromResult(Result.Success());

    public Task<Result> SendOtpAsync(string userEmail, string otp, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success());

    public Task<Result> SendPasswordResetEmailAsync(string userEmail, string token, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success());
}

public class TestCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime Expiry)> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var cached))
        {
            if (DateTime.UtcNow <= cached.Expiry)
            {
                return Task.FromResult((T?)cached.Value);
            }
            _cache.Remove(key);
        }
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var expiryTime = expiration.HasValue 
            ? DateTime.UtcNow.Add(expiration.Value) 
            : DateTime.UtcNow.AddHours(1);
            
        _cache[key] = (value, expiryTime);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
        return Task.CompletedTask;
    }
}

public class TestAuthenService : IAuthenService
{
    private readonly ICacheService _cacheService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, (string Password, string UserId, string Role)> _usersByEmail = new();
    private readonly Dictionary<string, string> _confirmationTokens = new();

    public TestAuthenService(ICacheService cacheService, IServiceProvider serviceProvider)
    {
        _cacheService = cacheService;
        _serviceProvider = serviceProvider;
    }

    public Task<Result<(string UserId, string confirmToken)>> CreateUserAsync(string email, string password, string role)
    {
        return CreateUserAsync(email, password, role, true);
    }

    public Task<Result<(string UserId, string confirmToken)>> CreateUserAsync(string email, string password, string role, bool requireEmailConfirmation)
    {
        var userId = Guid.NewGuid().ToString();
        _usersByEmail[email] = (password, userId, role);
        Console.WriteLine($"[TestAuthenService] CreateUserAsync: {email} -> UserId: {userId}");
        
        var confirmToken = requireEmailConfirmation ? $"token_{userId}" : "";
        _confirmationTokens[userId] = confirmToken;
        
        return Task.FromResult(Result<(string, string)>.Success((userId, confirmToken)));
    }

    public async Task<Result> LoginStepOneAsync(string email, string password)
    {
        Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: {email}");
        
        // Find user by email
        if (!_usersByEmail.TryGetValue(email, out var userInfo))
        {
            Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: User not found in dictionary");
            return Result.Failure(new Error("Account.NotFound", "Account not found", ErrorType.NotFound));
        }

        if (userInfo.Password != password)
        {
            Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: Password mismatch");
            return Result.Failure(new Error("Auth.InvalidCredentials", "Invalid credentials", ErrorType.Validation));
        }

        // Send OTP
        var otpKey = $"login_otp_{email}";
        await _cacheService.SetAsync(otpKey, "123456", TimeSpan.FromMinutes(5));
        Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: OTP cached with key {otpKey}");
        
        return Result.Success();
    }

    public async Task<Result> LoginStepOneAsync(string email, string password, string expectedRole)
    {
        Console.WriteLine($"[TestAuthenService] LoginStepOneAsync with role: {email}, expected role: {expectedRole}");
        
        // Find user by email
        if (!_usersByEmail.TryGetValue(email, out var userInfo))
        {
            Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: User not found in dictionary");
            return Result.Failure(new Error("Account.NotFound", "Account not found", ErrorType.NotFound));
        }

        if (userInfo.Password != password)
        {
            Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: Password mismatch");
            return Result.Failure(new Error("Auth.InvalidCredentials", "Invalid credentials", ErrorType.Validation));
        }

        // Check role
        if (userInfo.Role != expectedRole)
        {
            Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: Role mismatch. User role: {userInfo.Role}, expected: {expectedRole}");
            return Result.Failure(new Error("Account.RoleMismatch", "Role mismatch", ErrorType.Forbidden));
        }

        // Send OTP
        var otpKey = $"login_otp_{email}";
        await _cacheService.SetAsync(otpKey, "123456", TimeSpan.FromMinutes(5));
        Console.WriteLine($"[TestAuthenService] LoginStepOneAsync: OTP cached with key {otpKey}");
        
        return Result.Success();
    }

    public async Task<Result<(AuthResultDTO AuthResult, string CacheKey)>> LoginStepTwoAsync(string email, string otp)
    {
        var otpKey = $"login_otp_{email}";
        var cachedOtp = await _cacheService.GetAsync<string>(otpKey);
        
        if (cachedOtp != otp)
        {
            return Result<(AuthResultDTO, string)>.Failure(new Error("Auth.InvalidOtp", "Invalid OTP", ErrorType.Validation));
        }
        
        await _cacheService.RemoveAsync(otpKey);
        
        // Check OTP first
        Console.WriteLine($"[TestAuthenService] LoginStepTwoAsync: Checking OTP for {email}, expected: 123456, received: {otp}");
        
        // Find the stored user by email
        Console.WriteLine($"[TestAuthenService] LoginStepTwoAsync: Looking for {email}");
        Console.WriteLine($"[TestAuthenService] Available users: {string.Join(", ", _usersByEmail.Keys)}");
        
        if (!_usersByEmail.TryGetValue(email, out var userInfo))
        {
            Console.WriteLine($"[TestAuthenService] LoginStepTwoAsync: User not found in dictionary");
            return Result<(AuthResultDTO, string)>.Failure(new Error("Account.NotFound", "Account not found", ErrorType.NotFound));
        }
        
        var (password, userId, role) = userInfo;
        Console.WriteLine($"[TestAuthenService] LoginStepTwoAsync: Found {email} -> UserId: {userId}");
        
        var token = GenerateTestJwtToken(userId, email, role);
        
        var authResult = new AuthResultDTO(
            Token: token,
            RefreshToken: $"mock_refresh_token_{userId}",
            AspNetUserId: userId
        );
        
        return Result<(AuthResultDTO, string)>.Success((authResult, $"cache_key_{userId}"));
    }

    public Task<Result<string>> ConfirmEmailAsync(string userId, string confirmToken)
    {
        if (_confirmationTokens.TryGetValue(userId, out var expectedToken) && expectedToken == confirmToken)
        {
            return Task.FromResult(Result<string>.Success("Email confirmed successfully"));
        }
        return Task.FromResult(Result<string>.Failure(new Error("Auth.InvalidToken", "Invalid token", ErrorType.Validation)));
    }

    public Task<Result<ClaimsPrincipal?>> GetPrincipalFromExpiredToken(string token)
    {
        // Extract the user ID from the refresh token (format: mock_refresh_token_{userId})
        if (token.StartsWith("mock_refresh_token_"))
        {
            var userId = token.Substring("mock_refresh_token_".Length);
            
            // Find the user by userId in our dictionary
            var userEntry = _usersByEmail.FirstOrDefault(kvp => kvp.Value.UserId == userId);
            if (userEntry.Key != null)
            {
                var (password, aspNetUserId, role) = userEntry.Value;
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, aspNetUserId),
                    new Claim(ClaimTypes.Email, userEntry.Key),
                    new Claim(ClaimTypes.Role, role)
                };
                var identity = new ClaimsIdentity(claims, "test");
                var principal = new ClaimsPrincipal(identity);
                
                return Task.FromResult(Result<ClaimsPrincipal?>.Success(principal));
            }
        }
        
        // If token is invalid or user not found
        return Task.FromResult(Result<ClaimsPrincipal?>.Failure(new Error("Auth.InvalidToken", "Invalid token", ErrorType.Validation)));
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    public Task<string> GenerateAccessTokenAsync(string userId)
    {
        return Task.FromResult($"mock_access_token_{userId}");
    }

    public async Task<Result<string>> GeneratePasswordResetTokenAsync(string email)
    {
        var resetToken = Guid.NewGuid().ToString();
        var resetKey = $"password_reset_{email}";
        await _cacheService.SetAsync(resetKey, resetToken, TimeSpan.FromMinutes(15));
        return Result<string>.Success(resetToken);
    }

    public Task<Result> ResetPasswordAsync(string email, string newPassword)
    {
        // Mock implementation for testing
        // In real scenarios, this would update the user's password
        return Task.FromResult(Result.Success());
    }

    private string GenerateTestJwtToken(string userId, string email, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("AccountId", Guid.NewGuid().ToString())
        };

        // Add PartnerId for partners
        if (role == "Partner")
        {
            claims.Add(new Claim("PartnerId", Guid.NewGuid().ToString()));
        }
        else if (role == "User")
        {
            claims.Add(new Claim("UserId", Guid.NewGuid().ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key-that-is-long-enough-for-jwt-token-generation"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

#endregion