using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Property;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Infrastructure.Persistence;

namespace TripEnjoy.Infrastructure.Persistence.Repositories;

public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
    public PropertyRepository(TripEnjoyDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Property>> GetPropertiesByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken)
    {
        var partnerIdValueObject = PartnerId.Create(partnerId);

        return await _dbContext.Set<Property>()
            .Where(p => p.PartnerId == partnerIdValueObject)
            .Include(p => p.PropertyType)
            .Include(p => p.PropertyImages)
            .ToListAsync(cancellationToken);
    }

    public async Task<Property?> GetByIdWithImagesAsync(PropertyId propertyId)
    {
        return await _dbContext.Set<Property>()
            .Include(p => p.PropertyImages)
            .Include(p => p.PropertyType)
            .FirstOrDefaultAsync(p => p.Id == propertyId);
    }

    public async Task<(IEnumerable<Property> Properties, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<Property>()
            .Include(p => p.PropertyType)
            .Include(p => p.PropertyImages)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var properties = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (properties, totalCount);
    }
}
