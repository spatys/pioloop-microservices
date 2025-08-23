using MediatR;
using Property.Application.DTOs;

namespace Property.Application.Queries;

public class SearchPropertiesQuery : IRequest<PropertySearchResultDto>
{
    public PropertySearchDto SearchCriteria { get; }

    public SearchPropertiesQuery(PropertySearchDto searchCriteria)
    {
        SearchCriteria = searchCriteria;
    }
}
