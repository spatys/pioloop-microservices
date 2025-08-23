using PropertyEntity = Property.Domain.Entities.Property;

namespace Property.Domain.Repositories;

public interface IPropertyRepository
{
    Task<PropertyEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<PropertyEntity>> GetAllAsync();
    Task<IEnumerable<PropertyEntity>> GetByOwnerIdAsync(Guid ownerId);
    Task<(IEnumerable<PropertyEntity> Properties, int TotalCount)> SearchWithSimpleCriteriaAsync(
        string? location = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        int? guests = null,
        int page = 1,
        int pageSize = 20
    );
    Task<PropertyEntity> AddAsync(PropertyEntity property);
    Task<PropertyEntity> UpdateAsync(PropertyEntity property);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
