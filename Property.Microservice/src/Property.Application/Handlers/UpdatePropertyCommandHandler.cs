using MediatR;
using Property.Application.Commands;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Domain.Interfaces;
using AutoMapper;
using Property.Infrastructure.Services;

namespace Property.Application.Handlers;

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly IImageService _imageService;

    public UpdatePropertyCommandHandler(IPropertyRepository propertyRepository, IMapper mapper, IImageService imageService)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
        _imageService = imageService;
    }

    public async Task<PropertyResponse> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var existingProperty = await _propertyRepository.GetByIdAsync(request.Id);
        if (existingProperty == null)
        {
            throw new InvalidOperationException($"Property with ID {request.Id} not found");
        }

        // Mise à jour des propriétés
        existingProperty.Title = request.UpdatePropertyRequest.Title;
        existingProperty.Description = request.UpdatePropertyRequest.Description;
        existingProperty.PropertyType = request.UpdatePropertyRequest.PropertyType;
        existingProperty.MaxGuests = request.UpdatePropertyRequest.MaxGuests;
        existingProperty.Bedrooms = request.UpdatePropertyRequest.Bedrooms;
        existingProperty.Beds = request.UpdatePropertyRequest.Beds;
        existingProperty.Bathrooms = request.UpdatePropertyRequest.Bathrooms;
        existingProperty.SquareMeters = request.UpdatePropertyRequest.SquareMeters;
                    existingProperty.Address = request.UpdatePropertyRequest.Address;
            existingProperty.Neighborhood = request.UpdatePropertyRequest.Neighborhood;
            existingProperty.City = request.UpdatePropertyRequest.City;
            existingProperty.PostalCode = request.UpdatePropertyRequest.PostalCode;
        existingProperty.Latitude = request.UpdatePropertyRequest.Latitude ?? 0;
        existingProperty.Longitude = request.UpdatePropertyRequest.Longitude ?? 0;
        existingProperty.PricePerNight = request.UpdatePropertyRequest.PricePerNight;
        existingProperty.CleaningFee = request.UpdatePropertyRequest.CleaningFee;
        existingProperty.ServiceFee = request.UpdatePropertyRequest.ServiceFee;
        existingProperty.UpdatedAt = DateTime.UtcNow;

        // Gérer la mise à jour des images si fournies
        if (request.UpdatePropertyRequest.Images?.Any() == true)
        {
            // Supprimer les anciennes images de Vercel Blob
            await _imageService.DeletePropertyImagesAsync(existingProperty.Id.ToString());

            // Supprimer les anciennes images de la base de données
            existingProperty.Images.Clear();

            // Uploader et sauvegarder les nouvelles images
            var imageTasks = request.UpdatePropertyRequest.Images.Select(async (img, index) =>
            {
                // Convertir base64 en stream
                var imageBytes = Convert.FromBase64String(img.ImageUrl);
                var imageStream = new MemoryStream(imageBytes);
                
                // Upload vers Vercel Blob
                var fileName = $"image_{index + 1}.jpg";
                var imageUrl = await _imageService.UploadImageAsync(imageStream, fileName, existingProperty.Id.ToString());
                
                return new PropertyImage
                {
                    Id = Guid.NewGuid(),
                    PropertyId = existingProperty.Id,
                    ImageUrl = imageUrl,
                    AltText = img.AltText,
                    IsMainImage = img.IsMainImage,
                    DisplayOrder = img.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
            });

            existingProperty.Images = (await Task.WhenAll(imageTasks)).ToList();
        }

        var updatedProperty = await _propertyRepository.UpdateAsync(existingProperty);

        return _mapper.Map<PropertyResponse>(updatedProperty);
    }
}
