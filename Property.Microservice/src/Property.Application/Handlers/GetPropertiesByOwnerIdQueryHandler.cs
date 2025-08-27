using MediatR;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using Property.Domain.Interfaces;

namespace Property.Application.Handlers;

public class GetPropertiesByOwnerIdQueryHandler : IRequestHandler<GetPropertiesByOwnerIdQuery, IEnumerable<PropertyResponse>>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetPropertiesByOwnerIdQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<IEnumerable<PropertyResponse>> Handle(GetPropertiesByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var properties = await _propertyRepository.GetByOwnerIdAsync(request.OwnerId);
        
        return properties.Select(p => new PropertyResponse
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            PropertyType = p.PropertyType,
            MaxGuests = p.MaxGuests,
            Bedrooms = p.Bedrooms,
            Beds = p.Beds,
            Bathrooms = p.Bathrooms,
            SquareMeters = p.SquareMeters,
            Address = p.Address,
            City = p.City,
            PostalCode = p.PostalCode,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            PricePerNight = p.PricePerNight,
            CleaningFee = p.CleaningFee,
            ServiceFee = p.ServiceFee,
            Status = p.Status.ToString(),
            OwnerId = p.OwnerId,
            ImageUrls = p.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
            Amenities = p.Amenities?.Select(a => a.Name).ToList() ?? new List<string>(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }
}
