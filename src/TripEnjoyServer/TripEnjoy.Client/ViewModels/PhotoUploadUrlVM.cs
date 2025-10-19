namespace TripEnjoy.Client.ViewModels;

public class PhotoUploadUrlVM
{
    public string UploadUrl { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public Dictionary<string, string> UploadParameters { get; set; } = new();
    public long Timestamp { get; set; }
    public string Signature { get; set; } = string.Empty;
}