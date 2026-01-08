using System.Net.Http.Json;
using TripEnjoy.Client.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Client.Services;

public interface IPaymentService
{
    Task<ApiResponse<string>> ProcessPaymentAsync(ProcessPaymentRequest request);
    Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(Guid paymentId);
}

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;

    public PaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<string>> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/payments/process", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<string>>()
                   ?? new ApiResponse<string> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(Guid paymentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/payments/{paymentId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<PaymentStatusDto>>()
                   ?? new ApiResponse<PaymentStatusDto> { IsSuccess = false, Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaymentStatusDto> { IsSuccess = false, Message = ex.Message };
        }
    }
}
