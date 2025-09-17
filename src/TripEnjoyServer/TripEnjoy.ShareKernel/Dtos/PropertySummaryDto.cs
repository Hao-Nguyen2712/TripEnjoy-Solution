namespace TripEnjoy.ShareKernel.Dtos;

public class PropertySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PropertyTypeName { get; set; } = string.Empty;
    public decimal? AverageRating { get; set; }
    public string? CoverImageUrl { get; set; }
}
