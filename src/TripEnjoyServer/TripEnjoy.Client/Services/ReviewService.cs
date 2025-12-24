using System.Net.Http.Json;
using TripEnjoy.Client.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Client.Services;

public interface IReviewService
{
    Task<ApiResponse<Guid>> CreateReviewAsync(CreateReviewRequest request);
    Task<ApiResponse<bool>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request);
    Task<ApiResponse<bool>> DeleteReviewAsync(Guid reviewId);
    Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(Guid reviewId);
    Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsByPropertyAsync(Guid propertyId, int page = 1, int pageSize = 10);
    Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsByRoomTypeAsync(Guid roomTypeId, int page = 1, int pageSize = 10);
    Task<ApiResponse<PagedResult<ReviewDto>>> GetUserReviewsAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<ApiResponse<Guid>> CreateReplyAsync(Guid reviewId, CreateReplyRequest request);
    Task<ApiResponse<bool>> UpdateReplyAsync(Guid reviewId, Guid replyId, UpdateReplyRequest request);
    Task<ApiResponse<bool>> DeleteReplyAsync(Guid reviewId, Guid replyId);
}

public class ReviewService : IReviewService
{
    private readonly HttpClient _httpClient;

    public ReviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<Guid>> CreateReviewAsync(CreateReviewRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/reviews", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>()
                   ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/reviews/{reviewId}", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? new ApiResponse<bool> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> DeleteReviewAsync(Guid reviewId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/reviews/{reviewId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? new ApiResponse<bool> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(Guid reviewId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/reviews/{reviewId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReviewDto>>()
                   ?? new ApiResponse<ReviewDto> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReviewDto> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsByPropertyAsync(Guid propertyId, int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/reviews/property/{propertyId}?pageNumber={page}&pageSize={pageSize}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ReviewDto>>>()
                   ?? new ApiResponse<PagedResult<ReviewDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<ReviewDto>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsByRoomTypeAsync(Guid roomTypeId, int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/reviews/room-type/{roomTypeId}?pageNumber={page}&pageSize={pageSize}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ReviewDto>>>()
                   ?? new ApiResponse<PagedResult<ReviewDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<ReviewDto>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<PagedResult<ReviewDto>>> GetUserReviewsAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/reviews/user/{userId}?pageNumber={page}&pageSize={pageSize}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ReviewDto>>>()
                   ?? new ApiResponse<PagedResult<ReviewDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<ReviewDto>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<Guid>> CreateReplyAsync(Guid reviewId, CreateReplyRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/reviews/{reviewId}/replies", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>()
                   ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> UpdateReplyAsync(Guid reviewId, Guid replyId, UpdateReplyRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/reviews/{reviewId}/replies/{replyId}", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? new ApiResponse<bool> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> DeleteReplyAsync(Guid reviewId, Guid replyId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/reviews/{reviewId}/replies/{replyId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? new ApiResponse<bool> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { IsSuccess = false, Message = ex.Message };
        }
    }
}
