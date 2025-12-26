namespace TripEnjoy.ShareKernel.Dtos;

public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatorType { get; set; } = string.Empty;
    public Guid CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<VoucherTargetDto> Targets { get; set; } = new();
}

public class VoucherTargetDto
{
    public Guid Id { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public Guid? PartnerId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? RoomTypeId { get; set; }
}
