using MediatR;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using Property.Domain.Interfaces;
using Property.Domain.Models;

namespace Property.Application.Handlers;

public class SearchPropertiesQueryHandler : IRequestHandler<SearchPropertiesQuery, PropertySearchResponse>
{
    private readonly IPropertyRepository _propertyRepository;

    public SearchPropertiesQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertySearchResponse> Handle(SearchPropertiesQuery request, CancellationToken cancellationToken)
    {
        // Conversion du DTO en critères de recherche du domaine
        var criteria = new PropertySearchCriteria
        {
            Location = request.SearchCriteria.Location,
            CheckInDate = request.SearchCriteria.CheckInDate,
            CheckOutDate = request.SearchCriteria.CheckOutDate,
            Guests = request.SearchCriteria.Guests,
            Page = request.SearchCriteria.Page,
            PageSize = request.SearchCriteria.PageSize
        };

        // Appel au repository
        var result = await _propertyRepository.SearchAsync(criteria);

        // Conversion du résultat en DTO
        return new PropertySearchResponse
        {
            Properties = result.Properties.Select(p => new PropertyResponse
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
                Neighborhood = p.Neighborhood,
                City = p.City,
                PostalCode = p.PostalCode,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                PricePerNight = p.PricePerNight,
                CleaningFee = p.CleaningFee,
                ServiceFee = p.ServiceFee,
                Status = p.Status.ToString(),
                OwnerId = p.OwnerId,
                ImageUrls = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
                Amenities = p.Amenities.OrderBy(a => a.DisplayOrder).Select(a => a.Name).ToList(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }
}
