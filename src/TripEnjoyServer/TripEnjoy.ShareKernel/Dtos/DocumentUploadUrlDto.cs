namespace TripEnjoy.ShareKernel.Dtos;

public record DocumentUploadUrlDto(
    string UploadUrl,
    string PublicId,
    Dictionary<string, string> UploadParameters,
    long Timestamp,
    string Signature);