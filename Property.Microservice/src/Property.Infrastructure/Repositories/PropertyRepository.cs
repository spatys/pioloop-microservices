using Microsoft.EntityFrameworkCore;
using Property.Domain.Entities;
using Property.Domain.Interfaces;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Infrastructure.Data;

namespace Property.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly PropertyDbContext _context;

    public PropertyRepository(PropertyDbContext context)
    {
        _context = context;
    }

    public async Task<PropertySearchResponse> SearchAsync(PropertySearchCriteriaRequest criteria)
    {
        var query = _context.Properties
            .Include(p => p.Owner)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Amenities.OrderBy(a => a.DisplayOrder))
            .Where(p => p.Status == PropertyStatus.Published); // Seulement les propriétés publiées

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

        if (criteria.MinPrice.HasValue)
            query = query.Where(p => p.PricePerNight >= criteria.MinPrice.Value);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(p => p.PricePerNight <= criteria.MaxPrice.Value);

        if (!string.IsNullOrEmpty(criteria.PropertyType))
            query = query.Where(p => p.PropertyType.ToLower().Contains(criteria.PropertyType.ToLower()));

        if (!string.IsNullOrEmpty(criteria.RoomType))
            query = query.Where(p => p.RoomType.ToLower().Contains(criteria.RoomType.ToLower()));

        // Filtre par équipements
        if (criteria.Amenities != null && criteria.Amenities.Any())
        {
            foreach (var amenity in criteria.Amenities)
            {
                query = query.Where(p => p.Amenities.Any(a => 
                    a.Name.ToLower().Contains(amenity.ToLower())));
            }
        }

        // Filtre par disponibilité (si dates fournies)
        if (criteria.CheckIn.HasValue && criteria.CheckOut.HasValue)
        {
            query = query.Where(p => !p.Availability.Any(a => 
                a.CheckInDate <= criteria.CheckOut.Value && 
                a.CheckOutDate >= criteria.CheckIn.Value && 
                !a.IsAvailable));
        }

        // Compter le total avant pagination
        var totalCount = await query.CountAsync();

        // Pagination
        var properties = await query
            .OrderBy(p => p.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        // Calculer le nombre total de pages
        var totalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize);

        return new PropertySearchResponse
        {
            Properties = properties.Select(p => new PropertyResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                PropertyType = p.PropertyType,
                RoomType = p.RoomType,
                MaxGuests = p.MaxGuests,
                Bedrooms = p.Bedrooms,
                Beds = p.Beds,
                Bathrooms = p.Bathrooms,
                Address = p.Address,
                City = p.City,
                PostalCode = p.PostalCode,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                PricePerNight = p.PricePerNight,
                CleaningFee = p.CleaningFee,
                ServiceFee = p.ServiceFee,
                IsInstantBookable = p.IsInstantBookable,
                Status = p.Status.ToString(),
                OwnerId = p.OwnerId,
                OwnerName = $"{p.Owner?.FirstName} {p.Owner?.LastName}",
                OwnerEmail = p.Owner?.Email ?? string.Empty,
                ImageUrls = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
                Amenities = p.Amenities.OrderBy(a => a.DisplayOrder).Select(a => a.Name).ToList(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = criteria.Page,
            PageSize = criteria.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        return await _context.Properties
            .Include(p => p.Owner)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Amenities.OrderBy(a => a.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Property> AddAsync(Property property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<Property> UpdateAsync(Property property)
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

    public async Task<IEnumerable<Property>> GetByOwnerIdAsync(Guid ownerId)
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
}
