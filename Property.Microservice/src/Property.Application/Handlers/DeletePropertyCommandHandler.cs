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
    private readonly IBlobStorageService _blobStorageService;

    public DeletePropertyCommandHandler(
        IPropertyRepository propertyRepository, 
        IMapper mapper,
        IBlobStorageService blobStorageService)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
    }

    public async Task<PropertyResponse> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.Id);
        if (property == null)
        {
            throw new InvalidOperationException("Propriété non trouvée");
        }

        // Delete images from Vercel Blob before marking property as deleted
        if (property.Images?.Any() == true)
        {
            foreach (var image in property.Images)
            {
                try
                {
                    // Extract file path from URL for deletion
                    var urlParts = image.ImageUrl.Split('/');
                    if (urlParts.Length >= 3)
                    {
                        var filePath = $"{urlParts[^2]}/{urlParts[^1]}"; // images/{propertyId}/{fileName}
                        await _blobStorageService.DeleteImageAsync(filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    Console.WriteLine($"Error deleting image from Vercel Blob: {ex.Message}");
                }
            }
        }

        // Marquer la propriété comme supprimée (soft delete)
        property.Status = Property.Domain.Enums.PropertyStatus.Deleted;
        property.UpdatedAt = DateTime.UtcNow;

        var deletedProperty = await _propertyRepository.UpdateAsync(property);
        return _mapper.Map<PropertyResponse>(deletedProperty);
    }
}
