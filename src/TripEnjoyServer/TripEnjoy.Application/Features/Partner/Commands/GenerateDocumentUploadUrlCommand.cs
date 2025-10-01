using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Partner.Commands;

public record GenerateDocumentUploadUrlCommand(
    string DocumentType,
    string FileName) : IAuditableCommand<Result<DocumentUploadUrlDto>>;