using MediatR;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using Property.Domain.Interfaces;
using AutoMapper;

namespace Property.Application.Handlers;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyResponse?>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<PropertyResponse?> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.Id);
        
        if (property == null)
            return null;

        return _mapper.Map<PropertyResponse>(property);
    }
}
