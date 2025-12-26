using System.Net.Http.Json;
using TripEnjoy.Client.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Client.Services;

public interface IBookingService
{
    Task<ApiResponse<Guid>> CreateBookingAsync(CreateBookingRequest request);
    Task<ApiResponse<PagedList<BookingDto>>> GetMyBookingsAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse<BookingDetailDto>> GetBookingDetailsAsync(Guid bookingId);
    Task<ApiResponse<bool>> CancelBookingAsync(Guid bookingId, string? cancellationReason = null);
}

public class BookingService : IBookingService
{
    private readonly HttpClient _httpClient;

    public BookingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<Guid>> CreateBookingAsync(CreateBookingRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/bookings", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>() 
                   ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<PagedList<BookingDto>>> GetMyBookingsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/bookings/my-bookings?page={page}&pageSize={pageSize}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<PagedList<BookingDto>>>()
                   ?? new ApiResponse<PagedList<BookingDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedList<BookingDto>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<BookingDetailDto>> GetBookingDetailsAsync(Guid bookingId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/bookings/{bookingId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<BookingDetailDto>>()
                   ?? new ApiResponse<BookingDetailDto> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<BookingDetailDto> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> CancelBookingAsync(Guid bookingId, string? cancellationReason = null)
    {
        try
        {
            var request = new { CancellationReason = cancellationReason };
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/bookings/{bookingId}/cancel", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? new ApiResponse<bool> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { IsSuccess = false, Message = ex.Message };
        }
    }
}
