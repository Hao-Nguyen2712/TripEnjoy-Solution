using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.PropertyImage.Commands;

public record AddPropertyImageCommand(
    Guid PropertyId,
    string PublicId,
    string ImageUrl,
    string Signature,
    long Timestamp,
    bool IsCover,
    string? Caption = null) : IAuditableCommand<Result<PropertyImageId>>;
