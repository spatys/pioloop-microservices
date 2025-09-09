using Property.Domain.Entities;
using Property.Domain.Enums;
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
    
    // Opérations de disponibilité
    Task<IEnumerable<PropertyAvailability>> GetPropertyAvailabilitiesAsync(Guid propertyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<PropertyAvailability?> GetAvailabilityByIdAsync(Guid id);
    Task<PropertyAvailability> AddAvailabilityAsync(PropertyAvailability availability);
    Task<PropertyAvailability> UpdateAvailabilityAsync(PropertyAvailability availability);
    Task<bool> DeleteAvailabilityAsync(Guid id);
    Task<bool> HasAvailabilityConflictAsync(Guid propertyId, DateTime checkIn, DateTime checkOut, Guid? excludeId = null);
    Task SaveChangesAsync();
}
