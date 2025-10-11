using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.Test.IntegrationTests.WebApplicationFactory;
using Xunit;

namespace TripEnjoy.Test.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<TripEnjoyWebApplicationFactory>, IAsyncLifetime
{
    protected readonly TripEnjoyWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions;

    protected BaseIntegrationTest(TripEnjoyWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Get the database context for the current test scope
    /// </summary>
    protected TripEnjoyDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TripEnjoyDbContext>();
        
        // Ensure database is created and seeded with test data on first access
        EnsureDatabaseSeeded(context);
        
        return context;
    }

    private static bool _isSeeded = false;
    private static readonly object _seedLock = new object();

    private void EnsureDatabaseSeeded(TripEnjoyDbContext context)
    {
        if (_isSeeded) return;

        lock (_seedLock)
        {
            if (_isSeeded) return;

            context.Database.EnsureCreated();

            // Seed PropertyTypes if not exists
            if (!context.PropertyTypes.Any())
            {
                var propertyTypes = new[]
                {
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Hotel").Value,
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Apartment").Value,
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Resort").Value,
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Villa").Value,
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Cabin").Value,
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Guest House").Value,
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Hostel").Value,
                    TripEnjoy.Domain.PropertyType.PropertyType.Create("Motel").Value
                };

                context.PropertyTypes.AddRange(propertyTypes);
                context.SaveChanges();
            }

            _isSeeded = true;
        }
    }

    /// <summary>
    /// Helper method to create JSON content for HTTP requests
    /// </summary>
    protected static StringContent CreateJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Helper method to deserialize HTTP response content
    /// </summary>
    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Clean up the database after each test
    /// </summary>
    protected virtual Task CleanupDatabaseAsync()
    {
        // Default implementation does nothing, can be overridden by test classes
        return Task.CompletedTask;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await CleanupDatabaseAsync();
    }
    
    /// <summary>
         /// Set authorization header for authenticated requests
         /// </summary>
    protected void SetAuthorizationToken(string token)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clear authorization header
    /// </summary>
    protected void ClearAuthorizationToken()
    {
        HttpClient.DefaultRequestHeaders.Authorization = null;
    }
}