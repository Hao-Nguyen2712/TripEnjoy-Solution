using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using TripEnjoy.Client.Models;

namespace TripEnjoy.Client.Services;

public interface IAuthenticationService
{
    Task<ApiResponse<AuthResult>> LoginStepOneAsync(LoginRequest request);
    Task<ApiResponse<AuthResult>> LoginStepTwoAsync(VerifyOtpRequest request);
    Task<ApiResponse> RegisterAsync(RegisterRequest request);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<AuthResult>> RefreshTokenAsync();
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    public AuthenticationService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<ApiResponse<AuthResult>> LoginStepOneAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login-step-one", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
            return result ?? new ApiResponse<AuthResult> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuthResult>
            {
                IsSuccess = false,
                Message = "An error occurred during login",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<AuthResult>> LoginStepTwoAsync(VerifyOtpRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login-step-two", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
            
            if (result?.IsSuccess == true && result.Data != null)
            {
                await _localStorage.SetItemAsync(AccessTokenKey, result.Data.Token);
                await _localStorage.SetItemAsync(RefreshTokenKey, result.Data.RefreshToken);
            }
            
            return result ?? new ApiResponse<AuthResult> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuthResult>
            {
                IsSuccess = false,
                Message = "An error occurred during OTP verification",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/register", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred during registration",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/forgot-password", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/reset-password", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<AuthResult>> RefreshTokenAsync()
    {
        try
        {
            var accessToken = await _localStorage.GetItemAsync<string>(AccessTokenKey);
            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return new ApiResponse<AuthResult> { IsSuccess = false, Message = "No tokens found" };
            }

            var request = new RefreshTokenRequest
            {
                ExpiredAccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/refresh-token", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();

            if (result?.IsSuccess == true && result.Data != null)
            {
                await _localStorage.SetItemAsync(AccessTokenKey, result.Data.Token);
                await _localStorage.SetItemAsync(RefreshTokenKey, result.Data.RefreshToken);
            }

            return result ?? new ApiResponse<AuthResult> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuthResult>
            {
                IsSuccess = false,
                Message = "Failed to refresh token",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(AccessTokenKey);
    }
}
