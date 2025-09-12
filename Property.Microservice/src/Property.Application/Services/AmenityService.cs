using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Infrastructure.Data;

namespace Property.Application.Services;

public class AmenityService : IAmenityService
{
    private readonly PropertyDbContext _context;
    private readonly IMapper _mapper;

    public AmenityService(PropertyDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AmenityResponse>> GetAllAmenitiesAsync()
    {
        var amenities = await _context.Amenities
            .Where(a => a.IsActive)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AmenityResponse>>(amenities);
    }

    public async Task<IEnumerable<AmenityResponse>> GetAmenitiesByCategoryAsync(string category)
    {
        var amenities = await _context.Amenities
            .Where(a => a.IsActive && a.Category == category)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AmenityResponse>>(amenities);
    }

    public async Task<AmenityResponse?> GetAmenityByIdAsync(int id)
    {
        var amenity = await _context.Amenities
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

        return amenity != null ? _mapper.Map<AmenityResponse>(amenity) : null;
    }
}
