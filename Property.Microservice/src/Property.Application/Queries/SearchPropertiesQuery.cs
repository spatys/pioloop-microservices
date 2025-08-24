using MediatR;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;

namespace Property.Application.Queries;

public class SearchPropertiesQuery : IRequest<PropertySearchResponse>
{
    public PropertySearchCriteriaRequest SearchCriteria { get; }

    public SearchPropertiesQuery(PropertySearchCriteriaRequest searchCriteria)
    {
        SearchCriteria = searchCriteria;
    }
}
