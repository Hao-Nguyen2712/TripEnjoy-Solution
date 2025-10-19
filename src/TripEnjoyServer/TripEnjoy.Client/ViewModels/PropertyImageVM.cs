namespace TripEnjoy.Client.ViewModels
{
    public class PropertyImageVM
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; } = null;  // Not currently stored in database
        public bool IsMain { get; set; }  // To match API DTO
        public int DisplayOrder { get; set; } = 0;   // Not currently stored in database, using default
        public DateTime UploadAt { get; set; }       // Added from actual domain model
        
        // Alias property for backward compatibility with views that use IsPrimary
        public bool IsPrimary 
        { 
            get => IsMain; 
            set => IsMain = value; 
        }
    }
}