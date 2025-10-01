using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Interfaces.External.CloudStorage;

public interface ICloudinaryService
{
    /// <summary>
    /// Generates signed upload parameters for secure direct upload to Cloudinary
    /// </summary>
    /// <param name="folder">The folder path where the file will be stored</param>
    /// <param name="fileName">The original file name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload parameters including URL, signature, and other required fields</returns>
    Task<DocumentUploadUrlDto> GenerateSignedUploadParametersAsync(
        string folder,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a file was successfully uploaded to Cloudinary
    /// </summary>
    /// <param name="publicId">The public ID of the uploaded file</param>
    /// <param name="signature">The signature used for upload</param>
    /// <param name="timestamp">The timestamp used for upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the file exists and is valid</returns>
    Task<bool> ValidateUploadedFileAsync(
        string publicId,
        string signature,
        long timestamp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the secure URL for an uploaded file
    /// </summary>
    /// <param name="publicId">The public ID of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The secure HTTPS URL of the file</returns>
    Task<string> GetSecureUrlAsync(string publicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from Cloudinary
    /// </summary>
    /// <param name="publicId">The public ID of the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the file was successfully deleted</returns>
    Task<bool> DeleteFileAsync(string publicId, CancellationToken cancellationToken = default);
}