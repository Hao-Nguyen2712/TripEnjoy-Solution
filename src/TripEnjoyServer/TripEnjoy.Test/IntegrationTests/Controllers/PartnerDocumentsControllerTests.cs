using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.ShareKernel.Dtos;
using TripEnjoy.Test.IntegrationTests.WebApplicationFactory;

namespace TripEnjoy.Test.IntegrationTests.Controllers;

[Collection("IntegrationTest")]
public class PartnerDocumentsControllerTests : BaseIntegrationTest
{
    public PartnerDocumentsControllerTests(TripEnjoyWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDocuments_WithValidPartnerToken_ShouldReturnPagedDocuments()
    {
        // Arrange
        var (partnerId, token) = await CreatePartnerWithTokenAsync();
        await CreateTestDocumentsAsync(partnerId, 15);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedList<PartnerDocumentDto>>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Status.Should().Be("success");
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Items.Should().HaveCount(10);
        apiResponse.Data.TotalCount.Should().Be(15);
        apiResponse.Data.PageNumber.Should().Be(1);
        apiResponse.Data.PageSize.Should().Be(10);
        apiResponse.Data.HasNextPage.Should().BeTrue();
        apiResponse.Data.HasPreviousPage.Should().BeFalse();

        // Verify documents are ordered by latest submission date
        var items = apiResponse.Data.Items.ToList();
        for (int i = 0; i < items.Count - 1; i++)
        {
            items[i].CreatedAt.Should().BeAfter(items[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task GetDocuments_WithSecondPage_ShouldReturnCorrectPage()
    {
        // Arrange
        var (partnerId, token) = await CreatePartnerWithTokenAsync();
        await CreateTestDocumentsAsync(partnerId, 25);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents?pageNumber=2&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedList<PartnerDocumentDto>>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Items.Should().HaveCount(10);
        apiResponse.Data.TotalCount.Should().Be(25);
        apiResponse.Data.PageNumber.Should().Be(2);
        apiResponse.Data.HasNextPage.Should().BeTrue();
        apiResponse.Data.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetDocuments_WithEmptyResult_ShouldReturnEmptyPagedList()
    {
        // Arrange
        var (_, token) = await CreatePartnerWithTokenAsync(); // Partner with no documents

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedList<PartnerDocumentDto>>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Items.Should().BeEmpty();
        apiResponse.Data.TotalCount.Should().Be(0);
        apiResponse.Data.HasNextPage.Should().BeFalse();
        apiResponse.Data.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetDocuments_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDocuments_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDocuments_WithUserRole_ShouldReturnForbidden()
    {
        // Arrange
        var token = await CreateUserTokenAsync(); // User role instead of Partner role
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDocuments_WithCustomPageSize_ShouldRespectPageSize()
    {
        // Arrange
        var (partnerId, token) = await CreatePartnerWithTokenAsync();
        await CreateTestDocumentsAsync(partnerId, 20);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedList<PartnerDocumentDto>>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Items.Should().HaveCount(5);
        apiResponse.Data.PageSize.Should().Be(5);
        apiResponse.Data.TotalCount.Should().Be(20);
    }

    [Fact]
    public async Task GetDocuments_ShouldIncludeFormattedDisplayNames()
    {
        // Arrange
        var (partnerId, token) = await CreatePartnerWithTokenAsync();
        await CreateSpecificTestDocumentsAsync(partnerId);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/partner/documents?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedList<PartnerDocumentDto>>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        var items = apiResponse!.Data.Items.ToList();

        // Find specific document types and verify their display names
        var businessLicense = items.FirstOrDefault(d => d.DocumentType == "BusinessLicense");
        businessLicense.Should().NotBeNull();
        businessLicense!.DocumentTypeName.Should().Be("Business License");

        var taxDoc = items.FirstOrDefault(d => d.DocumentType == "TaxIdentification");
        taxDoc.Should().NotBeNull();
        taxDoc!.DocumentTypeName.Should().Be("Tax Identification");

        // Verify status display names
        var pendingDoc = items.FirstOrDefault(d => d.Status == "PendingReview");
        pendingDoc.Should().NotBeNull();
        pendingDoc!.StatusDisplayName.Should().Be("Pending Review");
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page number
    [InlineData(1, 0)]  // Invalid page size
    [InlineData(-1, 5)] // Negative page number
    [InlineData(1, -5)] // Negative page size
    public async Task GetDocuments_WithInvalidPagination_ShouldHandleGracefully(int pageNumber, int pageSize)
    {
        // Arrange
        var (partnerId, token) = await CreatePartnerWithTokenAsync();
        await CreateTestDocumentsAsync(partnerId, 10);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/partner/documents?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        // The API should handle invalid pagination gracefully and return OK
        // The actual behavior depends on the implementation (could return empty results or apply defaults)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDocuments_WithNonExistentPage_ShouldReturnEmptyResults()
    {
        // Arrange
        var (partnerId, token) = await CreatePartnerWithTokenAsync();
        await CreateTestDocumentsAsync(partnerId, 5);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Request page 10 when only 1 page exists
        var response = await HttpClient.GetAsync("/api/v1/partner/documents?pageNumber=10&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedList<PartnerDocumentDto>>>(content, JsonOptions);

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Items.Should().BeEmpty();
        apiResponse.Data.TotalCount.Should().Be(5); // Still shows correct total count
        apiResponse.Data.PageNumber.Should().Be(10);
    }

    #region Helper Methods

    private async Task<(PartnerId partnerId, string token)> CreatePartnerWithTokenAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TripEnjoyDbContext>();

        // Create account
        var account = Account.Create($"partner-{Guid.NewGuid()}", $"partner{Guid.NewGuid()}@test.com").Value;

        // Create partner
        var partnerId = PartnerId.CreateUnique();
        var partner = new Domain.Account.Entities.Partner(partnerId, account.Id, "Test Company", "123456789", "Test Address");

        // Associate partner with account using reflection
        var partnerProperty = typeof(Account).GetProperty("Partner");
        partnerProperty?.SetValue(account, partner);

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        // Generate JWT token for the partner
        var token = await GeneratePartnerTokenAsync(account.Id.Id, partnerId.Id);

        return (partnerId, token);
    }

    private async Task<string> CreateUserTokenAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TripEnjoyDbContext>();

        // Create regular user account
        var account = Account.Create($"user-{Guid.NewGuid()}", $"user{Guid.NewGuid()}@test.com").Value;

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        // Generate JWT token for the user (no partner role)
        var token = await GenerateUserTokenAsync(account.Id.Id);
        return token;
    }

    private async Task CreateTestDocumentsAsync(PartnerId partnerId, int count)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TripEnjoyDbContext>();

        var documentTypes = new[] { "BusinessLicense", "TaxIdentification", "ProofOfAddress", "CompanyRegistration", "BankStatement", "IdentityDocument" };
        var statuses = new[] { "PendingReview", "Approved", "Rejected" };

        var documents = new List<PartnerDocument>();
        for (int i = 0; i < count; i++)
        {
            var document = new PartnerDocument(
                PartnerDocumentId.CreateUnique(),
                partnerId,
                documentTypes[i % documentTypes.Length],
                $"https://cloudinary.com/test-{Guid.NewGuid()}",
                statuses[i % statuses.Length]);

            // Set different creation times to test ordering
            var createdAtProperty = typeof(PartnerDocument).GetProperty("CreatedAt");
            createdAtProperty?.SetValue(document, DateTime.UtcNow.AddMinutes(-i));

            documents.Add(document);
        }

        dbContext.Set<PartnerDocument>().AddRange(documents);
        await dbContext.SaveChangesAsync();
    }

    private async Task CreateSpecificTestDocumentsAsync(PartnerId partnerId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TripEnjoyDbContext>();

        var documents = new[]
        {
            new PartnerDocument(PartnerDocumentId.CreateUnique(), partnerId, "BusinessLicense", "https://cloudinary.com/business", "PendingReview"),
            new PartnerDocument(PartnerDocumentId.CreateUnique(), partnerId, "TaxIdentification", "https://cloudinary.com/tax", "Approved"),
            new PartnerDocument(PartnerDocumentId.CreateUnique(), partnerId, "ProofOfAddress", "https://cloudinary.com/address", "Rejected")
        };

        dbContext.Set<PartnerDocument>().AddRange(documents);
        await dbContext.SaveChangesAsync();
    }

    private async Task<string> GeneratePartnerTokenAsync(Guid accountId, Guid partnerId)
    {
        // Use the TestAuthenService to generate proper JWT tokens
        using var scope = Factory.Services.CreateScope();
        var authenService = scope.ServiceProvider.GetRequiredService<IAuthenService>() as TestAuthenService;

        if (authenService != null)
        {
            // Generate a proper JWT token using the test service
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, $"partner-user-{accountId}"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, $"partner{accountId}@test.com"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Partner"),
                new System.Security.Claims.Claim("AccountId", accountId.ToString()),
                new System.Security.Claims.Claim("PartnerId", partnerId.ToString())
            };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("test-secret-key-that-is-long-enough-for-jwt-token-generation"));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: "TestIssuer",
                audience: "TestAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        // This should not happen in tests
        throw new InvalidOperationException("TestAuthenService not available for token generation");
    }

    private async Task<string> GenerateUserTokenAsync(Guid accountId)
    {
        // Generate a proper JWT token for user
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, $"user-{accountId}"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, $"user{accountId}@test.com"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "User"),
            new System.Security.Claims.Claim("AccountId", accountId.ToString())
        };

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("test-secret-key-that-is-long-enough-for-jwt-token-generation"));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion

    // Helper classes for JSON deserialization
    public class ApiResponse<T>
    {
        public T Data { get; set; } = default!;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

}