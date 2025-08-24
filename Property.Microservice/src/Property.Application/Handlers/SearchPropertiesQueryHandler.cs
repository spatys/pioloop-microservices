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
                OwnerName = string.Empty, // À récupérer depuis Auth.Microservice si nécessaire
                OwnerEmail = string.Empty, // À récupérer depuis Auth.Microservice si nécessaire
                ImageUrls = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
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
