using MediatR;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using Property.Domain.Interfaces;
using AutoMapper;

namespace Property.Application.Handlers;

public class GetPropertiesByOwnerIdQueryHandler : IRequestHandler<GetPropertiesByOwnerIdQuery, IEnumerable<PropertyResponse>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public GetPropertiesByOwnerIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PropertyResponse>> Handle(GetPropertiesByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var properties = await _propertyRepository.GetByOwnerIdAsync(request.OwnerId);
        
        return _mapper.Map<IEnumerable<PropertyResponse>>(properties);
    }
}
