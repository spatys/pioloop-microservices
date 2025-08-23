using MediatR;
using Property.Application.DTOs;
using Property.Application.Queries;
using Property.Domain.Repositories;
using AutoMapper;

namespace Property.Application.Handlers;

public class GetAllPropertiesQueryHandler : IRequestHandler<GetAllPropertiesQuery, IEnumerable<PropertyResponse>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public GetAllPropertiesQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PropertyResponse>> Handle(GetAllPropertiesQuery request, CancellationToken cancellationToken)
    {
        var properties = await _propertyRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PropertyResponse>>(properties);
    }
}
