using Microsoft.EntityFrameworkCore;
using Property.Domain.Entities;
using Property.Domain.Enums;
using Property.Domain.Interfaces;
using Property.Domain.Models;
using Property.Infrastructure.Data;
using PropertyEntity = Property.Domain.Entities.Property;

namespace Property.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly PropertyDbContext _context;

    public PropertyRepository(PropertyDbContext context)
    {
        _context = context;
    }

    public async Task<PropertySearchResult> SearchAsync(PropertySearchCriteria criteria)
    {
        var query = _context.Properties
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Amenities.OrderBy(a => a.DisplayOrder))
            .Where(p => p.Status == PropertyStatus.Verified); // Seulement les propriétés vérifiées

        // Filtre par localisation (ex: "Bonabéri, Douala, Littoral")
        if (!string.IsNullOrEmpty(criteria.Location))
        {
            var location = criteria.Location.ToLower();
            query = query.Where(p => 
                p.City.ToLower().Contains(location) ||
                p.Address.ToLower().Contains(location) ||
                p.PostalCode.ToLower().Contains(location)
            );
        }

        if (criteria.Guests.HasValue)
            query = query.Where(p => p.MaxGuests >= criteria.Guests.Value);



        // Filtre par disponibilité (si dates fournies)
        if (criteria.CheckInDate.HasValue && criteria.CheckOutDate.HasValue)
        {
            query = query.Where(p => !p.Availability.Any(a => 
                a.CheckInDate <= criteria.CheckOutDate.Value && 
                a.CheckOutDate >= criteria.CheckInDate.Value && 
                !a.IsAvailable));
        }

        // Compter le total avant pagination
        var totalCount = await query.CountAsync();

        // Application du tri selon les critères
        query = ApplySorting(query, criteria.SortBy, criteria.SortOrder);

        // Pagination
        var properties = await query
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        // Calculer le nombre total de pages
        var totalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize);

        return new PropertySearchResult
        {
            Properties = properties,
            TotalCount = totalCount,
            Page = criteria.Page,
            PageSize = criteria.PageSize,
            TotalPages = totalPages
        };
    }

    private static IQueryable<PropertyEntity> ApplySorting(IQueryable<PropertyEntity> query, SortBy sortBy, SortOrder sortOrder)
    {
        return sortBy switch
        {
            SortBy.PricePerNight => sortOrder == SortOrder.Ascending 
                ? query.OrderBy(p => p.PricePerNight)
                : query.OrderByDescending(p => p.PricePerNight),
            
            SortBy.Popularity => sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.PopularityScore)
                : query.OrderByDescending(p => p.PopularityScore),
            
            SortBy.Rating => sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.AverageRating)
                : query.OrderByDescending(p => p.AverageRating),
            
            SortBy.ViewCount => sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.ViewCount)
                : query.OrderByDescending(p => p.ViewCount),
            
            SortBy.ReservationCount => sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.ReservationCount)
                : query.OrderByDescending(p => p.ReservationCount),
            
            SortBy.CreatedAt or _ => sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };
    }



    public async Task<PropertyEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Properties
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Amenities.OrderBy(a => a.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id);
    }



    public async Task<PropertyEntity> AddAsync(PropertyEntity property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<PropertyEntity> UpdateAsync(PropertyEntity property)
    {
        property.UpdatedAt = DateTime.UtcNow;
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null) return false;
        
        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PropertyEntity>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Properties
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Amenities.OrderBy(a => a.DisplayOrder))
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(Guid id, PropertyStatus status)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null) return false;

        property.Status = status;
        property.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return true;
    }

    // Opérations de disponibilité
    public async Task<IEnumerable<PropertyAvailability>> GetPropertyAvailabilitiesAsync(Guid propertyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.PropertyAvailabilities
            .Where(a => a.PropertyId == propertyId);

        if (startDate.HasValue)
            query = query.Where(a => a.CheckOutDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CheckInDate <= endDate.Value);

        return await query.OrderBy(a => a.CheckInDate).ToListAsync();
    }

    public async Task<PropertyAvailability?> GetAvailabilityByIdAsync(Guid id)
    {
        return await _context.PropertyAvailabilities.FindAsync(id);
    }

    public async Task<PropertyAvailability> AddAvailabilityAsync(PropertyAvailability availability)
    {
        _context.PropertyAvailabilities.Add(availability);
        return availability;
    }

    public async Task<PropertyAvailability> UpdateAvailabilityAsync(PropertyAvailability availability)
    {
        availability.UpdatedAt = DateTime.UtcNow;
        _context.PropertyAvailabilities.Update(availability);
        return availability;
    }

    public async Task<bool> DeleteAvailabilityAsync(Guid id)
    {
        var availability = await _context.PropertyAvailabilities.FindAsync(id);
        if (availability == null) return false;

        _context.PropertyAvailabilities.Remove(availability);
        return true;
    }

    public async Task<bool> HasAvailabilityConflictAsync(Guid propertyId, DateTime checkIn, DateTime checkOut, Guid? excludeId = null)
    {
        var query = _context.PropertyAvailabilities
            .Where(a => a.PropertyId == propertyId &&
                       a.CheckInDate < checkOut &&
                       a.CheckOutDate > checkIn);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
