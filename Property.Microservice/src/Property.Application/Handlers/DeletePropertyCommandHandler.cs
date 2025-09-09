using MediatR;
using Property.Application.Commands;
using Property.Application.DTOs.Response;
using Property.Domain.Interfaces;
using PropertyEntity = Property.Domain.Entities.Property;
using AutoMapper;

namespace Property.Application.Handlers;

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public DeletePropertyCommandHandler(
        IPropertyRepository propertyRepository, 
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<PropertyResponse> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.Id);
        if (property == null)
        {
            throw new InvalidOperationException("Propriété non trouvée");
        }

        // Images are stored as base64 in database, no need to delete from external storage

        // Marquer la propriété comme supprimée (soft delete)
        property.Status = Property.Domain.Enums.PropertyStatus.Deleted;
        property.UpdatedAt = DateTime.UtcNow;

        var deletedProperty = await _propertyRepository.UpdateAsync(property);
        return _mapper.Map<PropertyResponse>(deletedProperty);
    }
}
