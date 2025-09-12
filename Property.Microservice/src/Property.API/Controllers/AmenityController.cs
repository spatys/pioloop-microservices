using Microsoft.AspNetCore.Mvc;
using Property.Application.DTOs.Response;
using Property.Application.Services;

namespace Property.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AmenityController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenityController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    /// <summary>
    /// Récupère toutes les amenities disponibles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AmenityResponse>>> GetAllAmenities()
    {
        var amenities = await _amenityService.GetAllAmenitiesAsync();
        return Ok(amenities);
    }

    /// <summary>
    /// Récupère les amenities par catégorie
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<AmenityResponse>>> GetAmenitiesByCategory(string category)
    {
        var amenities = await _amenityService.GetAmenitiesByCategoryAsync(category);
        return Ok(amenities);
    }

    /// <summary>
    /// Récupère une amenity par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AmenityResponse>> GetAmenityById(int id)
    {
        var amenity = await _amenityService.GetAmenityByIdAsync(id);
        if (amenity == null)
        {
            return NotFound();
        }
        return Ok(amenity);
    }
}
