using TripEnjoy.Domain.Property;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Interfaces.Persistence;

public interface IPropertyRepository : IGenericRepository<Property>
{
    Task<IEnumerable<Property>> GetPropertiesByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken);
    Task<Property?> GetByIdWithImagesAsync(PropertyId propertyId);
    Task<(IEnumerable<Property> Properties, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
