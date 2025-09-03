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
    private readonly IBlobStorageService _blobStorageService;

    public UpdatePropertyCommandHandler(
        IPropertyRepository propertyRepository, 
        IMapper mapper,
        IBlobStorageService blobStorageService)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
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

        // Update images if provided - Delete old images and upload new ones to Vercel Blob
        if (request.UpdatePropertyRequest.Images?.Any() == true)
        {
            // Delete existing images from Vercel Blob
            if (existingProperty.Images?.Any() == true)
            {
                foreach (var existingImage in existingProperty.Images)
                {
                    try
                    {
                        // Extract file path from URL for deletion
                        var urlParts = existingImage.ImageUrl.Split('/');
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
            
            // Remove existing images from DB
            existingProperty.Images?.Clear();
            
            var propertyImages = new List<PropertyImage>();
            
            foreach (var img in request.UpdatePropertyRequest.Images)
            {
                try
                {
                    // Convert base64 to stream
                    var cleanImageData = img.ImageData;
                    if (img.ImageData.Contains("data:"))
                    {
                        cleanImageData = img.ImageData.Split(',')[1];
                    }
                    
                    var imageBytes = Convert.FromBase64String(cleanImageData);
                    var imageStream = new MemoryStream(imageBytes);
                    
                    // Generate unique filename
                    var fileExtension = GetFileExtension(img.ContentType);
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    
                    // Upload new image to Vercel Blob
                    var imageUrl = await _blobStorageService.UploadImageAsync(imageStream, fileName, existingProperty.Id.ToString());
                    
                    // Create PropertyImage entity with Vercel Blob URL
                    var propertyImage = new PropertyImage
                    {
                        Id = Guid.NewGuid(),
                        PropertyId = existingProperty.Id,
                        ImageUrl = imageUrl, // URL Vercel Blob
                        AltText = img.AltText,
                        IsMainImage = img.IsMainImage,
                        DisplayOrder = img.DisplayOrder,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    propertyImages.Add(propertyImage);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other images
                    Console.WriteLine($"Error processing image: {ex.Message}");
                }
            }
            
            existingProperty.Images = propertyImages;
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
