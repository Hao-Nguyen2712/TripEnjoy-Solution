using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.PropertyImage.Commands;

public record GeneratePhotoUploadUrlCommand(
    Guid PropertyId,
    string FileName,
    string? Caption = null) : IAuditableCommand<Result<PhotoUploadUrlDto>>;