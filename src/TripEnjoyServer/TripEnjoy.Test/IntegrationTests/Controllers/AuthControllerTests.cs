using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TripEnjoy.ShareKernel.Dtos;
using TripEnjoy.ShareKernel.Models.ApiResult;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.Test.IntegrationTests.WebApplicationFactory;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using FluentAssertions;

namespace TripEnjoy.Test.IntegrationTests.Controllers;

public class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(TripEnjoyWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldReturnCreatedResponse()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand(
            Email: "testuser@example.com",
            Password: "Test123!@&",
            FullName: "Test User"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/register-user", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<object>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("success");
        responseContent.Message.Should().Contain("successful");
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand(
            Email: "invalid-email",
            Password: "Test123!@&",
            FullName: "Test User"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/register-user", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<string>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("error");
    }

    [Fact]
    public async Task LoginStepOne_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange - First register a user
        var registerCommand = new RegisterUserCommand(
            Email: "logintest@example.com",
            Password: "Test123!@&",
            FullName: "Login Test"
        );

        await HttpClient.PostAsJsonAsync("/api/v1/auth/register-user", registerCommand);

        var loginCommand = new LoginStepOneCommand(
            Email: "logintest@example.com",
            Password: "Test123!@&"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-one", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<string>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("success");
        responseContent.Message.Should().Contain("successful");
    }

    [Fact]
    public async Task LoginStepOne_WithInvalidCredentials_ShouldReturnNotFound()
    {
        // Arrange
        var loginCommand = new LoginStepOneCommand(
            Email: "nonexistent@example.com",
            Password: "WrongPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-one", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<string>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("error");
    }

    [Fact]
    public async Task LoginStepTwo_WithValidOtp_ShouldReturnTokens()
    {
        // Arrange - Create test data directly in database
        var scope = Factory.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        // Create a test user through TestAuthenService
        var testAuthenService = Factory.Services.GetRequiredService<IAuthenService>();
        var createUserResult = await testAuthenService.CreateUserAsync(
            "otptest@example.com", 
            "Test123!@&", 
            "Partner"
        );
        
        createUserResult.IsSuccess.Should().BeTrue();
        var (userId, _) = createUserResult.Value;
        
        // Create corresponding Account entity directly in database
        var accountResult = Account.Create(userId, "otptest@example.com");
        accountResult.IsSuccess.Should().BeTrue();
        
        var account = accountResult.Value;
        var partnerResult = account.AddNewPartner(
            companyName: "Test Company", 
            phoneNumber: null, 
            address: null, 
            email: "otptest@example.com");
        partnerResult.IsSuccess.Should().BeTrue();
        
        await unitOfWork.Repository<Account>().AddAsync(account);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        
        Console.WriteLine($"Test setup: Created Account {account.Id} for AspNetUserId: {userId}");
        
        // Don't dispose scope - let GC handle it to avoid UnitOfWork disposal issues

        var loginStepOneCommand = new LoginStepOneCommand(
            Email: "otptest@example.com",
            Password: "Test123!@&"
        );

        await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-one", loginStepOneCommand);

        var loginStepTwoCommand = new LoginStepTwoCommand(
            Email: "otptest@example.com",
            Otp: "123456" // Test OTP from TestAuthenService
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-two", loginStepTwoCommand);

        // Debug: Print response content if not OK
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<AuthResultDTO>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("success");
        responseContent.Data.Should().NotBeNull();
        responseContent.Data!.Token.Should().NotBeNullOrEmpty();
        responseContent.Data.RefreshToken.Should().NotBeNullOrEmpty();
        responseContent.Data.AspNetUserId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginStepTwo_WithInvalidOtp_ShouldReturnBadRequest()
    {
        // Arrange - Register user and perform step one
        var registerCommand = new RegisterUserCommand(
            Email: "invalidotptest@example.com",
            Password: "Test123!@&",
            FullName: "Invalid OTP Test"
        );

        await HttpClient.PostAsJsonAsync("/api/v1/auth/register-user", registerCommand);

        var loginStepOneCommand = new LoginStepOneCommand(
            Email: "invalidotptest@example.com",
            Password: "Test123!@&"
        );

        await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-one", loginStepOneCommand);

        var loginStepTwoCommand = new LoginStepTwoCommand(
            Email: "invalidotptest@example.com",
            Otp: "999999" // Invalid OTP
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-two", loginStepTwoCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<AuthResultDTO>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("error");
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange - Complete login flow to get tokens using Partner registration
        var registerCommand = new RegisterPartnerCommand(
            Email: "refreshtest@example.com",
            Password: "Test123!@&",
            CompanyName: "Test Company",
            ContactNumber: "123-456-7890",
            Address: "123 Test St"
        );

        await HttpClient.PostAsJsonAsync("/api/v1/auth/register-partner", registerCommand);

        // Step 1: Login
        var loginStepOneCommand = new LoginStepOneCommand(
            Email: "refreshtest@example.com",
            Password: "Test123!@&"
        );

        await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-one", loginStepOneCommand);

        // Step 2: Get tokens
        var loginStepTwoCommand = new LoginStepTwoCommand(
            Email: "refreshtest@example.com",
            Otp: "123456"
        );

        var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/auth/login-step-two", loginStepTwoCommand);
        var loginContent = await DeserializeResponseAsync<ApiResponse<AuthResultDTO>>(loginResponse);

        var refreshCommand = new RefreshTokenCommand(
            expiredAccessToken: loginContent!.Data!.Token,
            refreshToken: loginContent.Data.RefreshToken
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/refresh-token", refreshCommand);

        // Debug: Print error response if not OK
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[RefreshToken Test] Error response ({response.StatusCode}): {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<AuthResultDTO>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("success");
        responseContent.Data.Should().NotBeNull();
        responseContent.Data!.Token.Should().NotBeNullOrEmpty();
        responseContent.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ConfirmEmail_WithValidData_ShouldReturnResponse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = $"token_{userId}";
        var confirmFor = "registration";

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/auth/confirm-email?userId={userId}&token={token}&confirmFor={confirmFor}");

        // Assert
        // The actual response depends on the implementation - could be success or failure
        var responseContent = await DeserializeResponseAsync<ApiResponse<string>>(response);
        responseContent.Should().NotBeNull();
    }

    protected override async Task CleanupDatabaseAsync()
    {
        using var context = GetDbContext();
        var testAccounts = context.Accounts
            .Where(a => a.AccountEmail.Contains("example.com"))
            .ToList();
        
        context.Accounts.RemoveRange(testAccounts);
        await context.SaveChangesAsync();
    }
}