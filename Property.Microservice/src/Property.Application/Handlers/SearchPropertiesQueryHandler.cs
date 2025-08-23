using MediatR;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using Property.Domain.Interfaces;

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
            Location = request.SearchCriteria.Location, // Ex: "Bonabéri, Douala, Littoral"
            CheckIn = request.SearchCriteria.CheckInDate,
            CheckOut = request.SearchCriteria.CheckOutDate,
            Guests = request.SearchCriteria.Guests,
            Page = request.SearchCriteria.Page,
            PageSize = request.SearchCriteria.PageSize
        };

        // Appel au repository
        var result = await _propertyRepository.SearchAsync(criteria);

        // Le repository retourne déjà PropertySearchResponse, pas besoin de conversion
        return result;
    }
}
