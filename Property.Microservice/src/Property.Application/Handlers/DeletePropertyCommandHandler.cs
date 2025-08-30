using MediatR;
using Property.Application.Commands;
using Property.Domain.Interfaces;
using Property.Domain.Enums;
using Property.Infrastructure.Services;

namespace Property.Application.Handlers;

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IImageService _imageService;

    public DeletePropertyCommandHandler(IPropertyRepository propertyRepository, IImageService imageService)
    {
        _propertyRepository = propertyRepository;
        _imageService = imageService;
    }

    public async Task<bool> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.Id);
        if (property == null)
            return false;

        // Supprimer les images de Vercel Blob
        await _imageService.DeletePropertyImagesAsync(property.Id.ToString());

        property.Status = PropertyStatus.Deleted;
        property.UpdatedAt = DateTime.UtcNow;
        
        await _propertyRepository.UpdateAsync(property);
        return true;
    }
}
