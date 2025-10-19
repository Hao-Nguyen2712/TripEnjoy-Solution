using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Application.Interfaces.Persistence;

public interface IPartnerDocumentRepository : IGenericRepository<PartnerDocument>
{
    Task<(IEnumerable<PartnerDocument> Documents, int TotalCount)> GetDocumentsByPartnerIdPaginatedAsync(
        PartnerId partnerId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}