using System.ComponentModel.DataAnnotations;

namespace TripEnjoy.Client.ViewModels
{
    public class ManageImagesVM
    {
        public Guid PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public List<PropertyImageVM> Images { get; set; } = new();
        public AddImageVM NewImage { get; set; } = new();
        
        public PropertyImageVM? CoverImage => Images.FirstOrDefault(i => i.IsPrimary);
        public int TotalImages => Images.Count;
        public bool HasCoverImage => CoverImage != null;
    }
}