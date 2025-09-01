using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Property.Application.Commands;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Domain.Enums;
using Property.Domain.Interfaces;
using PropertyEntity = Property.Domain.Entities.Property;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Property.Infrastructure.Services;

namespace Property.Application.Handlers;

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IImageService _imageService;

    public CreatePropertyCommandHandler(
        IPropertyRepository propertyRepository, 
        IConfiguration configuration,
        HttpClient httpClient,
        IMapper mapper,
        IImageService imageService)
    {
        _propertyRepository = propertyRepository;
        _configuration = configuration;
        _httpClient = httpClient;
        _mapper = mapper;
        _imageService = imageService;
    }

    public async Task<PropertyResponse> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // L'ID utilisateur est déjà récupéré par le contrôleur depuis le header X-User-Id
            // et passé via le CreatePropertyCommand. Pas besoin de IHttpContextAccessor ici.
            var ownerId = request.UserId;

        var property = new PropertyEntity
        {
            Id = Guid.NewGuid(),
            Title = request.CreatePropertyRequest.Title,
            Description = request.CreatePropertyRequest.Description,
            PropertyType = request.CreatePropertyRequest.PropertyType,
            MaxGuests = request.CreatePropertyRequest.MaxGuests,
            Bedrooms = request.CreatePropertyRequest.Bedrooms,
            Beds = request.CreatePropertyRequest.Beds,
            Bathrooms = request.CreatePropertyRequest.Bathrooms,
            SquareMeters = request.CreatePropertyRequest.SquareMeters,
                            Address = request.CreatePropertyRequest.Address,
                Neighborhood = request.CreatePropertyRequest.Neighborhood,
                City = request.CreatePropertyRequest.City,
                PostalCode = request.CreatePropertyRequest.PostalCode,
            Latitude = request.CreatePropertyRequest.Latitude ?? 0,
            Longitude = request.CreatePropertyRequest.Longitude ?? 0,
            PricePerNight = request.CreatePropertyRequest.PricePerNight,
            CleaningFee = request.CreatePropertyRequest.CleaningFee,
            ServiceFee = request.CreatePropertyRequest.ServiceFee,
            Status = PropertyStatus.PendingApproval,
            OwnerId = ownerId, // Utiliser l'ID de l'utilisateur connecté
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add amenities if provided
        if (request.CreatePropertyRequest.Amenities?.Any() == true)
        {
            property.Amenities = request.CreatePropertyRequest.Amenities.Select(a => new PropertyAmenity
            {
                Id = Guid.NewGuid(),
                PropertyId = property.Id,
                Name = a.Name,
                Description = a.Description,
                Type = (AmenityType)a.Type,
                Category = (AmenityCategory)a.Category,
                IsAvailable = a.IsAvailable,
                IsIncludedInRent = a.IsIncludedInRent,
                AdditionalCost = a.AdditionalCost,
                Icon = a.Icon,
                Priority = a.Priority,
                DisplayOrder = a.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
        }

        // Add images if provided
        if (request.CreatePropertyRequest.Images?.Any() == true)
        {
            Console.WriteLine($"Processing {request.CreatePropertyRequest.Images.Count()} images...");
            var propertyImages = new List<PropertyImage>();
            
            foreach (var img in request.CreatePropertyRequest.Images)
            {
                try
                {
                    Console.WriteLine($"Processing image: {img.ImageUrl}");
                    
                    // Télécharger l'image depuis l'URL fournie
                    var imageResponse = await _httpClient.GetAsync(img.ImageUrl);
                    if (!imageResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to download image from {img.ImageUrl}: {imageResponse.StatusCode}");
                        continue;
                    }
                    
                    Console.WriteLine($"Image downloaded successfully from {img.ImageUrl}");
                    var imageStream = await imageResponse.Content.ReadAsStreamAsync();
                    var fileName = $"property_{property.Id}_{Guid.NewGuid()}.jpg";
                    
                    Console.WriteLine($"Uploading image to Vercel Blob with filename: {fileName}");
                    
                    // Uploader l'image dans Vercel Blob
                    var blobUrl = await _imageService.UploadImageAsync(imageStream, fileName, property.Id.ToString());
                    
                    Console.WriteLine($"Image uploaded successfully to Vercel Blob: {blobUrl}");
                    
                    // Créer l'entité PropertyImage avec l'URL Vercel Blob
                    var propertyImage = new PropertyImage
                    {
                        Id = Guid.NewGuid(),
                        PropertyId = property.Id,
                        ImageUrl = blobUrl, // URL Vercel Blob au lieu de l'URL externe
                        AltText = img.AltText,
                        IsMainImage = img.IsMainImage,
                        DisplayOrder = img.DisplayOrder,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    propertyImages.Add(propertyImage);
                    Console.WriteLine($"PropertyImage entity created with blob URL: {blobUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing image {img.ImageUrl}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Continuer avec les autres images
                }
            }
            
            Console.WriteLine($"Total images processed: {propertyImages.Count}");
            property.Images = propertyImages;
        }
        else
        {
            Console.WriteLine("No images provided in request");
        }

        var createdProperty = await _propertyRepository.AddAsync(property);

        // Vérifier que la propriété a été créée avec succès
        if (createdProperty == null)
        {
            throw new InvalidOperationException("La création de la propriété a échoué. Veuillez réessayer.");
        }

        // Mettre à jour le rôle de l'utilisateur vers "Owner" s'il était "Tenant"
        await UpdateUserRoleToOwner(ownerId);

        return _mapper.Map<PropertyResponse>(createdProperty);
    }
    catch (Exception ex)
    {
        // Log l'erreur pour le debugging
        Console.WriteLine($"Error creating property: {ex.Message}");
        
        // Relancer l'exception pour que l'interface puisse la gérer
        throw new InvalidOperationException($"Erreur lors de la création de la propriété : {ex.Message}");
    }
}



    private async Task UpdateUserRoleToOwner(Guid userId)
    {
        try
        {
            // Récupérer l'URL de l'API d'authentification
            var authApiUrl = _configuration["AuthApi:BaseUrl"] ?? "http://auth-api";
            


            // Appeler l'API d'authentification pour mettre à jour le rôle
            var response = await _httpClient.PostAsync($"{authApiUrl}/api/roles/assign?userId={userId}&roleName=Owner", null);
            
            if (!response.IsSuccessStatusCode)
            {
                // Log l'erreur mais ne pas faire échouer la création de propriété
                Console.WriteLine($"Failed to update user role to Owner for user {userId}. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // Log l'exception mais ne pas faire échouer la création de propriété
            Console.WriteLine($"Error updating user role to Owner for user {userId}: {ex.Message}");
        }
    }
}
