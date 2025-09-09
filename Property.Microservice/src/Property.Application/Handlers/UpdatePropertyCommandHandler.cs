using MediatR;
using Microsoft.Extensions.Configuration;
using Property.Application.Commands;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Domain.Enums;
using Property.Domain.Interfaces;
using PropertyEntity = Property.Domain.Entities.Property;
using AutoMapper;

namespace Property.Application.Handlers;

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public UpdatePropertyCommandHandler(
        IPropertyRepository propertyRepository, 
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<PropertyResponse> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var existingProperty = await _propertyRepository.GetByIdAsync(request.Id);
        if (existingProperty == null)
        {
            throw new InvalidOperationException("Propriété non trouvée");
        }

        // Update basic property information
        existingProperty.Title = !string.IsNullOrEmpty(request.UpdatePropertyRequest.Title) ? request.UpdatePropertyRequest.Title : existingProperty.Title;
        existingProperty.Description = !string.IsNullOrEmpty(request.UpdatePropertyRequest.Description) ? request.UpdatePropertyRequest.Description : existingProperty.Description;
        existingProperty.PropertyType = !string.IsNullOrEmpty(request.UpdatePropertyRequest.PropertyType) ? request.UpdatePropertyRequest.PropertyType : existingProperty.PropertyType;
        existingProperty.MaxGuests = request.UpdatePropertyRequest.MaxGuests > 0 ? request.UpdatePropertyRequest.MaxGuests : existingProperty.MaxGuests;
        existingProperty.Bedrooms = request.UpdatePropertyRequest.Bedrooms > 0 ? request.UpdatePropertyRequest.Bedrooms : existingProperty.Bedrooms;
        existingProperty.Beds = request.UpdatePropertyRequest.Beds > 0 ? request.UpdatePropertyRequest.Beds : existingProperty.Beds;
        existingProperty.Bathrooms = request.UpdatePropertyRequest.Bathrooms > 0 ? request.UpdatePropertyRequest.Bathrooms : existingProperty.Bathrooms;
        existingProperty.SquareMeters = request.UpdatePropertyRequest.SquareMeters > 0 ? request.UpdatePropertyRequest.SquareMeters : existingProperty.SquareMeters;
        existingProperty.Address = !string.IsNullOrEmpty(request.UpdatePropertyRequest.Address) ? request.UpdatePropertyRequest.Address : existingProperty.Address;
        existingProperty.Neighborhood = !string.IsNullOrEmpty(request.UpdatePropertyRequest.Neighborhood) ? request.UpdatePropertyRequest.Neighborhood : existingProperty.Neighborhood;
        existingProperty.City = !string.IsNullOrEmpty(request.UpdatePropertyRequest.City) ? request.UpdatePropertyRequest.City : existingProperty.City;
        existingProperty.PostalCode = !string.IsNullOrEmpty(request.UpdatePropertyRequest.PostalCode) ? request.UpdatePropertyRequest.PostalCode : existingProperty.PostalCode;
        existingProperty.Latitude = request.UpdatePropertyRequest.Latitude ?? existingProperty.Latitude;
        existingProperty.Longitude = request.UpdatePropertyRequest.Longitude ?? existingProperty.Longitude;
        existingProperty.PricePerNight = request.UpdatePropertyRequest.PricePerNight > 0 ? request.UpdatePropertyRequest.PricePerNight : existingProperty.PricePerNight;
        existingProperty.CleaningFee = request.UpdatePropertyRequest.CleaningFee > 0 ? request.UpdatePropertyRequest.CleaningFee : existingProperty.CleaningFee;
        existingProperty.ServiceFee = request.UpdatePropertyRequest.ServiceFee > 0 ? request.UpdatePropertyRequest.ServiceFee : existingProperty.ServiceFee;
        existingProperty.UpdatedAt = DateTime.UtcNow;

        // Update images if provided - Images are stored as BLOB in database
        if (request.UpdatePropertyRequest.Images?.Any() == true)
        {
            Console.WriteLine($"Updating property {existingProperty.Id} with {request.UpdatePropertyRequest.Images.Count} images");
            
            // Remove existing images from DB
            if (existingProperty.Images?.Any() == true)
            {
                Console.WriteLine($"Removing {existingProperty.Images.Count} existing images from DB");
                existingProperty.Images.Clear();
            }
            
            var propertyImages = new List<PropertyImage>();
            
            foreach (var img in request.UpdatePropertyRequest.Images)
            {
                Console.WriteLine($"Adding image with alt text: {img.AltText}");
                
                // Vérifier que les données base64 ne sont pas vides
                if (string.IsNullOrEmpty(img.ImageData))
                {
                    Console.WriteLine("Skipping image with empty base64 data");
                    continue;
                }
                
                // Convertir base64 en bytes
                byte[] imageBytes;
                try
                {
                    // Supprimer le préfixe "data:image/...;base64," si présent
                    var base64Data = img.ImageData;
                    if (base64Data.Contains(','))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }
                    
                    imageBytes = Convert.FromBase64String(base64Data);
                    Console.WriteLine($"Converted base64 to {imageBytes.Length} bytes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting base64 to bytes: {ex.Message}");
                    continue;
                }
                
                // Create PropertyImage entity with BLOB data
                var propertyImage = new PropertyImage
                {
                    Id = Guid.NewGuid(),
                    PropertyId = existingProperty.Id,
                    ImageData = imageBytes, // Store as BLOB
                    ContentType = img.ContentType,
                    AltText = img.AltText,
                    IsMainImage = img.IsMainImage,
                    DisplayOrder = img.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                
                propertyImages.Add(propertyImage);
            }
            
            existingProperty.Images = propertyImages;
            Console.WriteLine($"Property {existingProperty.Id} updated with {propertyImages.Count} BLOB images");
        }

        var updatedProperty = await _propertyRepository.UpdateAsync(existingProperty);
        return _mapper.Map<PropertyResponse>(updatedProperty);
    }

    private string GetFileExtension(string contentType)
    {
        return contentType.ToLower() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".jpg" // Default to jpg
        };
    }
}
