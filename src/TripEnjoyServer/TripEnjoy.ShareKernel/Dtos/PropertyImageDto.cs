namespace TripEnjoy.ShareKernel.Dtos;

public class PropertyImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public DateTime UploadAt { get; set; }
}
