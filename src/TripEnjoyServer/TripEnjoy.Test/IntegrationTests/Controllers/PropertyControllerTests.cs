using System.Net;
using TripEnjoy.ShareKernel.Models.ApiResult;
using TripEnjoy.Test.IntegrationTests.WebApplicationFactory;
using FluentAssertions;

namespace TripEnjoy.Test.IntegrationTests.Controllers;

public class PropertyControllerTests : BaseIntegrationTest
{
    public PropertyControllerTests(TripEnjoyWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProperties_ShouldReturnAllProperties()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/v1/properties");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await DeserializeResponseAsync<ApiResponse<object>>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Status.Should().Be("success");
    }

    protected override async Task CleanupDatabaseAsync()
    {
        // Minimal cleanup to avoid concurrency issues
        await Task.CompletedTask;
    }
}