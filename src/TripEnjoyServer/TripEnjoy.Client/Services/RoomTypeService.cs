using System.Net.Http.Json;
using TripEnjoy.Client.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Client.Services;

public interface IRoomTypeService
{
    Task<ApiResponse<List<RoomTypeDto>>> GetRoomTypesByPropertyAsync(Guid propertyId);
    Task<ApiResponse<RoomTypeDto>> GetRoomTypeByIdAsync(Guid roomTypeId);
    Task<ApiResponse<Guid>> CreateRoomTypeAsync(CreateRoomTypeRequest request);
    Task<ApiResponse> UpdateRoomTypeAsync(UpdateRoomTypeRequest request);
    Task<ApiResponse> DeleteRoomTypeAsync(Guid roomTypeId);
}

public class RoomTypeService : IRoomTypeService
{
    private readonly HttpClient _httpClient;

    public RoomTypeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<List<RoomTypeDto>>> GetRoomTypesByPropertyAsync(Guid propertyId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/room-types/property/{propertyId}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<RoomTypeDto>>>();
            return result ?? new ApiResponse<List<RoomTypeDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<RoomTypeDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching room types",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<RoomTypeDto>> GetRoomTypeByIdAsync(Guid roomTypeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/room-types/{roomTypeId}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<RoomTypeDto>>();
            return result ?? new ApiResponse<RoomTypeDto> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<RoomTypeDto>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching room type",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateRoomTypeAsync(CreateRoomTypeRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/room-types", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            return result ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                IsSuccess = false,
                Message = "An error occurred while creating room type",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> UpdateRoomTypeAsync(UpdateRoomTypeRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/room-types/{request.RoomTypeId}", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while updating room type",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> DeleteRoomTypeAsync(Guid roomTypeId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/room-types/{roomTypeId}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while deleting room type",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
