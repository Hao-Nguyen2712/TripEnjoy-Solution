using System.Net.Http.Json;
using TripEnjoy.Client.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Client.Services;

public interface IAdminService
{
    // Dashboard
    Task<ApiResponse<AdminDashboardStatsDto>> GetDashboardStatsAsync();
    
    // User Management
    Task<ApiResponse<List<UserManagementDto>>> GetAllUsersAsync();
    Task<ApiResponse> BlockUserAsync(Guid userId, string reason);
    Task<ApiResponse> UnblockUserAsync(Guid userId);
    
    // Partner Management
    Task<ApiResponse<List<PartnerApprovalDto>>> GetPendingPartnerApprovalsAsync();
    Task<ApiResponse> ApprovePartnerAsync(Guid partnerId);
    Task<ApiResponse> RejectPartnerAsync(Guid partnerId, string reason);
    
    // Property Management
    Task<ApiResponse<List<PropertyApprovalDto>>> GetPendingPropertyApprovalsAsync();
    Task<ApiResponse> ApprovePropertyAsync(Guid propertyId);
    Task<ApiResponse> RejectPropertyAsync(Guid propertyId, string reason);
    
    // Bookings Management
    Task<ApiResponse<List<BookingDetailDto>>> GetAllBookingsAsync();
    
    // Vouchers Management
    Task<ApiResponse<List<VoucherDto>>> GetAllVouchersAsync();
}

public class AdminService : IAdminService
{
    private readonly HttpClient _httpClient;

    public AdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<AdminDashboardStatsDto>> GetDashboardStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/dashboard/stats");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminDashboardStatsDto>>();
            return result ?? new ApiResponse<AdminDashboardStatsDto> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AdminDashboardStatsDto>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching dashboard stats",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<UserManagementDto>>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/users");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserManagementDto>>>();
            return result ?? new ApiResponse<List<UserManagementDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<UserManagementDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching users",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> BlockUserAsync(Guid userId, string reason)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/admin/users/{userId}/block", new { reason });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while blocking user",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> UnblockUserAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/admin/users/{userId}/unblock", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while unblocking user",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<PartnerApprovalDto>>> GetPendingPartnerApprovalsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/partners/pending");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PartnerApprovalDto>>>();
            return result ?? new ApiResponse<List<PartnerApprovalDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PartnerApprovalDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching pending partners",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> ApprovePartnerAsync(Guid partnerId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/admin/partners/{partnerId}/approve", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while approving partner",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> RejectPartnerAsync(Guid partnerId, string reason)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/admin/partners/{partnerId}/reject", new { reason });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while rejecting partner",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<PropertyApprovalDto>>> GetPendingPropertyApprovalsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/properties/pending");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PropertyApprovalDto>>>();
            return result ?? new ApiResponse<List<PropertyApprovalDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PropertyApprovalDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching pending properties",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> ApprovePropertyAsync(Guid propertyId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/admin/properties/{propertyId}/approve", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while approving property",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> RejectPropertyAsync(Guid propertyId, string reason)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/admin/properties/{propertyId}/reject", new { reason });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "An error occurred while rejecting property",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<BookingDetailDto>>> GetAllBookingsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/bookings");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookingDetailDto>>>();
            return result ?? new ApiResponse<List<BookingDetailDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<BookingDetailDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching bookings",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<VoucherDto>>> GetAllVouchersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/vouchers");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<VoucherDto>>>();
            return result ?? new ApiResponse<List<VoucherDto>> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<VoucherDto>>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching vouchers",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
