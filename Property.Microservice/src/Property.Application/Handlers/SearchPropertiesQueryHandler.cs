using MediatR;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using Property.Domain.Interfaces;
using Property.Domain.Models;
using AutoMapper;

namespace Property.Application.Handlers;

public class SearchPropertiesQueryHandler : IRequestHandler<SearchPropertiesQuery, PropertySearchResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public SearchPropertiesQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
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
            Properties = _mapper.Map<List<PropertyResponse>>(result.Properties),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }
}
