using Microsoft.EntityFrameworkCore;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Repositories;

public class PartnerDocumentRepository : GenericRepository<PartnerDocument>, IPartnerDocumentRepository
{
    public PartnerDocumentRepository(TripEnjoyDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IEnumerable<PartnerDocument> Documents, int TotalCount)> GetDocumentsByPartnerIdPaginatedAsync(
        PartnerId partnerId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<PartnerDocument>()
            .Where(d => d.PartnerId == partnerId)
            .OrderByDescending(d => d.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var documents = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (documents, totalCount);
    }
}