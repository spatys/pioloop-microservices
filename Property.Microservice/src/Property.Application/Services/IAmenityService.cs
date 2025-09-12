using Property.Application.DTOs.Response;

namespace Property.Application.Services;

public interface IAmenityService
{
    Task<IEnumerable<AmenityResponse>> GetAllAmenitiesAsync();
    Task<IEnumerable<AmenityResponse>> GetAmenitiesByCategoryAsync(string category);
    Task<AmenityResponse?> GetAmenityByIdAsync(int id);
}
