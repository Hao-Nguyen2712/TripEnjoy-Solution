namespace TripEnjoy.ShareKernel.Dtos;

/// <summary>
/// DTO for payment status
/// </summary>
public record PaymentStatusDto
{
    public Guid PaymentId { get; init; }
    public Guid BookingId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? TransactionId { get; init; }
    public DateTime? PaidAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
