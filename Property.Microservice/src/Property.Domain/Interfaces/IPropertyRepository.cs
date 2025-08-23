using Property.Domain.Entities;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;

namespace Property.Domain.Interfaces;

public interface IPropertyRepository
{
    // Recherche avec filtres (comme Airbnb)
    Task<PropertySearchResponse> SearchAsync(PropertySearchCriteriaRequest criteria);
    
    // Opérations CRUD de base
    Task<Property?> GetByIdAsync(Guid id);
    Task<Property> AddAsync(Property property);
    Task<Property> UpdateAsync(Property property);
    Task<bool> DeleteAsync(Guid id);
    
    // Opérations spécifiques
    Task<IEnumerable<Property>> GetByOwnerIdAsync(Guid ownerId);
    Task<bool> UpdateStatusAsync(Guid id, PropertyStatus status);
}
