using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Partner.Queries;

public record GetPartnerDocumentsQuery(int PageNumber, int PageSize) 
    : IRequest<Result<PagedList<PartnerDocumentDto>>>;