namespace TripEnjoy.Client.ViewModels
{
    public class PropertyDetailsVM
    {
        public Guid Id { get; set; }
        public Guid PropertyTypeId { get; set; }
        public string PropertyTypeName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PropertyImageVM> Images { get; set; } = new();

        public string FullAddress => $"{Address}, {City}, {Country}";
        public string StatusBadgeClass => Status.ToLower() switch
        {
            "active" => "bg-success",
            "pending" => "bg-warning",
            "inactive" => "bg-secondary",
            "rejected" => "bg-danger",
            _ => "bg-secondary"
        };
    }
}