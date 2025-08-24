using Property.Domain.Entities;
using Property.Domain.Models;
using PropertyEntity = Property.Domain.Entities.Property;

namespace Property.Domain.Interfaces;

public interface IPropertyRepository
{
    // Recherche avec filtres
    Task<PropertySearchResult> SearchAsync(PropertySearchCriteria criteria);
    
    // Opérations CRUD de base
    Task<PropertyEntity?> GetByIdAsync(Guid id);
    Task<PropertyEntity> AddAsync(PropertyEntity property);
    Task<PropertyEntity> UpdateAsync(PropertyEntity property);
    Task<bool> DeleteAsync(Guid id);
    
    // Opérations spécifiques
    Task<IEnumerable<PropertyEntity>> GetByOwnerIdAsync(Guid ownerId);
    Task<bool> UpdateStatusAsync(Guid id, PropertyStatus status);
}
