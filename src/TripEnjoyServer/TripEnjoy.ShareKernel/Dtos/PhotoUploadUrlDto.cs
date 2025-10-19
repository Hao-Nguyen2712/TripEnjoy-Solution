namespace TripEnjoy.ShareKernel.Dtos;

public record PhotoUploadUrlDto(
    string UploadUrl,
    string PublicId,
    Dictionary<string, string> UploadParameters,
    long Timestamp,
    string Signature);