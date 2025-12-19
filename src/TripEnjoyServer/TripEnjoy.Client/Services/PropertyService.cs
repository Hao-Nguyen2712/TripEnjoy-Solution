using System.Net.Http.Json;
using TripEnjoy.Client.Models;

namespace TripEnjoy.Client.Services;

public interface IPropertyService
{
    Task<ApiResponse<PropertyDto>> GetPropertyByIdAsync(Guid propertyId);
    Task<ApiResponse<PagedResult<PropertySummaryDto>>> GetAllPropertiesAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<List<PropertySummaryDto>>> GetMyPropertiesAsync();
    Task<ApiResponse<Guid>> CreatePropertyAsync(CreatePropertyRequest request);
    Task<ApiResponse> UpdatePropertyAsync(UpdatePropertyRequest request);
    Task<ApiResponse<List<PropertyTypeDto>>> GetPropertyTypesAsync();
}

public class PropertyService : IPropertyService
{
    private readonly HttpClient _httpClient;

    public PropertyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<PropertyDto>> GetPropertyByIdAsync(Guid propertyId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/properties/{propertyId}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PropertyDto>>();
            return result ?? new ApiResponse<PropertyDto> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PropertyDto>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching property",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<PropertySummaryDto>>> GetAllPropertiesAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/properties?pageNumber={pageNumber}&pageSize={pageSize}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<PropertySummaryDto>>>();
            return result ?? new ApiResponse<PagedResult<PropertySummaryDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<PropertySummaryDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching properties",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<PropertySummaryDto>>> GetMyPropertiesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/properties/mine");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PropertySummaryDto>>>();
            return result ?? new ApiResponse<List<PropertySummaryDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PropertySummaryDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching your properties",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreatePropertyAsync(CreatePropertyRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/properties", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            return result ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                IsSuccess = false,
                Message = "An error occurred while creating property",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> UpdatePropertyAsync(UpdatePropertyRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/properties/{request.PropertyId}", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while updating property",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<PropertyTypeDto>>> GetPropertyTypesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/property-types");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PropertyTypeDto>>>();
            return result ?? new ApiResponse<List<PropertyTypeDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PropertyTypeDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching property types",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
