namespace TripEnjoy.ShareKernel.Dtos;

public record TransactionDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public Guid? BookingId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record CreateTransactionRequest
{
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = null!;
    public Guid? BookingId { get; init; }
    public string? Description { get; init; }
}
