using System.ComponentModel.DataAnnotations;

namespace TripEnjoy.Client.ViewModels
{
    public class AddImageVM
    {
        [Required(ErrorMessage = "Public ID is required")]
        public string PublicId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Signature is required")]
        public string Signature { get; set; } = string.Empty;

        [Required(ErrorMessage = "Timestamp is required")]
        public long Timestamp { get; set; }

        [Display(Name = "Set as Cover Image")]
        public bool IsCover { get; set; } = false;

        [Display(Name = "Caption")]
        [StringLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
        public string? Caption { get; set; }

        // Additional fields for file upload
        [Display(Name = "File Name")]
        public string? FileName { get; set; }
    }
}