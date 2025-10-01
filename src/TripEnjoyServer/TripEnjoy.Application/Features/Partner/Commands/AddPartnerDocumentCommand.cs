using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Partner.Commands;

public record AddPartnerDocumentCommand(
    string DocumentType,
    string PublicId,
    string DocumentUrl,
    string Signature,
    long Timestamp) : IAuditableCommand<Result<PartnerDocumentId>>;
