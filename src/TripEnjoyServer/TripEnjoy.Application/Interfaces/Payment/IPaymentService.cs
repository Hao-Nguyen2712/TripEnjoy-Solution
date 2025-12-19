using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Interfaces.Payment;

/// <summary>
/// Payment service interface for processing payments
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create a payment URL for the user to complete payment
    /// </summary>
    /// <param name="orderId">The booking/order identifier</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="orderInfo">Order information/description</param>
    /// <param name="returnUrl">URL to redirect after payment completion</param>
    /// <returns>Payment URL</returns>
    Task<Result<string>> CreatePaymentUrlAsync(
        string orderId,
        decimal amount,
        string orderInfo,
        string returnUrl);

    /// <summary>
    /// Verify payment callback from payment gateway
    /// </summary>
    /// <param name="callbackData">Callback data from payment gateway</param>
    /// <returns>True if payment is successful, false otherwise</returns>
    Task<Result<PaymentCallbackResult>> VerifyPaymentCallbackAsync(
        Dictionary<string, string> callbackData);

    /// <summary>
    /// Process a refund for a payment
    /// </summary>
    /// <param name="transactionId">Original transaction ID</param>
    /// <param name="amount">Amount to refund</param>
    /// <param name="reason">Refund reason</param>
    /// <returns>Refund transaction ID</returns>
    Task<Result<string>> ProcessRefundAsync(
        string transactionId,
        decimal amount,
        string reason);
}

/// <summary>
/// Payment callback result
/// </summary>
public record PaymentCallbackResult(
    bool IsSuccess,
    string TransactionId,
    decimal Amount,
    string OrderId,
    string? Message = null);
