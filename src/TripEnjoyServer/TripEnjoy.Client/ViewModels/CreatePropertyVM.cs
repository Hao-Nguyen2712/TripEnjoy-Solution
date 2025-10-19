using System.ComponentModel.DataAnnotations;

namespace TripEnjoy.Client.ViewModels
{
    public class CreatePropertyVM
    {
        [Required(ErrorMessage = "Property type is required")]
        [Display(Name = "Property Type")]
        public Guid PropertyTypeId { get; set; }

        [Required(ErrorMessage = "Property name is required")]
        [StringLength(200, ErrorMessage = "Property name cannot exceed 200 characters")]
        [Display(Name = "Property Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Display(Name = "Latitude")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Display(Name = "Longitude")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }

        // For dropdown
        public List<PropertyTypeVM> PropertyTypes { get; set; } = new();
    }
}