namespace TripEnjoy.ShareKernel.Dtos;

public record SettlementDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal CommissionAmount { get; init; }
    public decimal NetAmount { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? PaidAt { get; init; }
}

public record ProcessSettlementRequest
{
    public Guid WalletId { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal CommissionPercentage { get; init; }
}
