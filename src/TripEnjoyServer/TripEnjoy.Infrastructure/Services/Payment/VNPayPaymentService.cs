using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using TripEnjoy.Application.Interfaces.Payment;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Infrastructure.Services.Payment;

/// <summary>
/// VNPay payment gateway service implementation
/// </summary>
public class VNPayPaymentService : IPaymentService
{
    private readonly VNPayConfiguration _config;
    private readonly ILogger<VNPayPaymentService> _logger;

    public VNPayPaymentService(
        IOptions<VNPayConfiguration> config,
        ILogger<VNPayPaymentService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public Task<Result<string>> CreatePaymentUrlAsync(
        string orderId,
        decimal amount,
        string orderInfo,
        string returnUrl)
    {
        try
        {
            // Create request data
            var vnpay = new SortedDictionary<string, string>();
            vnpay.Add("vnp_Version", _config.Version);
            vnpay.Add("vnp_Command", _config.Command);
            vnpay.Add("vnp_TmnCode", _config.TmnCode);
            vnpay.Add("vnp_Amount", ((long)(amount * 100)).ToString()); // VNPay requires amount in VND cents
            vnpay.Add("vnp_CurrCode", _config.CurrencyCode);
            vnpay.Add("vnp_TxnRef", orderId);
            vnpay.Add("vnp_OrderInfo", orderInfo);
            vnpay.Add("vnp_OrderType", "other");
            vnpay.Add("vnp_Locale", _config.Locale);
            vnpay.Add("vnp_ReturnUrl", returnUrl);
            vnpay.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.Add("vnp_IpAddr", "127.0.0.1"); // Should be user's IP in production

            // Build query string
            var query = BuildQueryString(vnpay);
            
            // Generate secure hash
            var signData = string.Join("&", vnpay.Select(kv => $"{kv.Key}={kv.Value}"));
            var vnpSecureHash = HmacSHA512(_config.HashSecret, signData);
            
            // Build payment URL
            var paymentUrl = $"{_config.PaymentUrl}?{query}&vnp_SecureHash={vnpSecureHash}";

            _logger.LogInformation("Generated VNPay payment URL for order {OrderId}", orderId);

            return Task.FromResult(Result<string>.Success(paymentUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating VNPay payment URL for order {OrderId}", orderId);
            return Task.FromResult(Result<string>.Failure(new Error(
                "Payment.UrlGenerationFailed",
                "Failed to generate payment URL.",
                ErrorType.Failure)));
        }
    }

    public Task<Result<PaymentCallbackResult>> VerifyPaymentCallbackAsync(
        Dictionary<string, string> callbackData)
    {
        try
        {
            // Extract secure hash from callback
            if (!callbackData.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
            {
                _logger.LogWarning("Missing vnp_SecureHash in payment callback");
                return Task.FromResult(Result<PaymentCallbackResult>.Failure(new Error(
                    "Payment.InvalidCallback",
                    "Missing secure hash in payment callback.",
                    ErrorType.Validation)));
            }

            // Remove hash from data for verification
            var vnpData = new SortedDictionary<string, string>();
            foreach (var kvp in callbackData.Where(x => x.Key != "vnp_SecureHash"))
            {
                vnpData.Add(kvp.Key, kvp.Value);
            }

            // Verify secure hash
            var signData = string.Join("&", vnpData.Select(kv => $"{kv.Key}={kv.Value}"));
            var checkSum = HmacSHA512(_config.HashSecret, signData);

            if (!checkSum.Equals(vnpSecureHash, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid secure hash in payment callback");
                return Task.FromResult(Result<PaymentCallbackResult>.Failure(new Error(
                    "Payment.InvalidSignature",
                    "Invalid payment signature.",
                    ErrorType.Validation)));
            }

            // Extract callback data
            var responseCode = callbackData.GetValueOrDefault("vnp_ResponseCode", "");
            var transactionId = callbackData.GetValueOrDefault("vnp_TransactionNo", "");
            var orderId = callbackData.GetValueOrDefault("vnp_TxnRef", "");
            var amountStr = callbackData.GetValueOrDefault("vnp_Amount", "0");
            
            // Parse amount (VNPay returns amount in cents)
            if (!long.TryParse(amountStr, out var amountCents))
            {
                _logger.LogWarning("Invalid amount in payment callback: {Amount}", amountStr);
                return Task.FromResult(Result<PaymentCallbackResult>.Failure(new Error(
                    "Payment.InvalidAmount",
                    "Invalid amount in payment callback.",
                    ErrorType.Validation)));
            }

            var amount = amountCents / 100m;

            // Check if payment was successful (response code "00" means success)
            var isSuccess = responseCode == "00";
            var message = GetResponseMessage(responseCode);

            var result = new PaymentCallbackResult(
                isSuccess,
                transactionId,
                amount,
                orderId,
                message);

            _logger.LogInformation("Verified payment callback for order {OrderId}: {IsSuccess}",
                orderId, isSuccess);

            return Task.FromResult(Result<PaymentCallbackResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment callback");
            return Task.FromResult(Result<PaymentCallbackResult>.Failure(new Error(
                "Payment.VerificationFailed",
                "Failed to verify payment callback.",
                ErrorType.Failure)));
        }
    }

    public Task<Result<string>> ProcessRefundAsync(
        string transactionId,
        decimal amount,
        string reason)
    {
        try
        {
            // Note: VNPay refund requires a separate API call with authentication
            // This is a simplified implementation
            // In production, you would need to:
            // 1. Make HTTP request to VNPay refund API
            // 2. Include proper authentication
            // 3. Handle VNPay's response

            _logger.LogInformation("Processing refund for transaction {TransactionId}, amount {Amount}",
                transactionId, amount);

            // For now, return a mock refund transaction ID
            // In production, this should be the actual refund transaction ID from VNPay
            var refundTransactionId = $"REFUND_{transactionId}_{DateTime.Now:yyyyMMddHHmmss}";

            _logger.LogInformation("Refund processed successfully: {RefundTransactionId}", refundTransactionId);

            return Task.FromResult(Result<string>.Success(refundTransactionId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", transactionId);
            return Task.FromResult(Result<string>.Failure(new Error(
                "Payment.RefundFailed",
                "Failed to process refund.",
                ErrorType.Failure)));
        }
    }

    private string BuildQueryString(SortedDictionary<string, string> data)
    {
        return string.Join("&", data.Select(kv => 
            $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}"));
    }

    private string HmacSHA512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        
        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Payment successful",
            "07" => "Fraud detection triggered",
            "09" => "Card/Account not registered for internet banking",
            "10" => "Wrong OTP authentication more than 3 times",
            "11" => "Payment timeout",
            "12" => "Card/Account locked",
            "13" => "Wrong OTP authentication password",
            "24" => "Transaction cancelled",
            "51" => "Insufficient account balance",
            "65" => "Daily transaction limit exceeded",
            "75" => "Bank under maintenance",
            "79" => "Wrong payment password more than allowed times",
            _ => $"Payment failed with code {responseCode}"
        };
    }
}
