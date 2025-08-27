using MediatR;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using Property.Domain.Interfaces;

namespace Property.Application.Handlers;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyResponse?>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyResponse?> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.Id);
        
        if (property == null)
            return null;

        return new PropertyResponse
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            PropertyType = property.PropertyType,
            MaxGuests = property.MaxGuests,
            Bedrooms = property.Bedrooms,
                            Beds = property.Beds,
                Bathrooms = property.Bathrooms,
                SquareMeters = property.SquareMeters,
                Address = property.Address,
                Neighborhood = property.Neighborhood,
                City = property.City,
                PostalCode = property.PostalCode,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                PricePerNight = property.PricePerNight,
                CleaningFee = property.CleaningFee,
                ServiceFee = property.ServiceFee,
                Status = property.Status.ToString(),
            OwnerId = property.OwnerId,
            ImageUrls = property.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
            Amenities = property.Amenities.OrderBy(a => a.DisplayOrder).Select(a => a.Name).ToList(),
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
    }
}
