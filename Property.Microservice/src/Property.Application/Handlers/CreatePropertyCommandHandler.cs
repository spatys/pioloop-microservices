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

namespace Property.Application.Handlers;

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IBlobStorageService _blobStorageService;

    public CreatePropertyCommandHandler(
        IPropertyRepository propertyRepository, 
        IConfiguration configuration,
        HttpClient httpClient,
        IMapper mapper,
        IBlobStorageService blobStorageService)
    {
        _propertyRepository = propertyRepository;
        _configuration = configuration;
        _httpClient = httpClient;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
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

            // Add images if provided - Upload to Vercel Blob
            if (request.CreatePropertyRequest.Images?.Any() == true)
            {
                var propertyImages = new List<PropertyImage>();
                
                foreach (var img in request.CreatePropertyRequest.Images)
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
                        
                        // Upload image to Vercel Blob
                        var imageUrl = await _blobStorageService.UploadImageAsync(imageStream, fileName, property.Id.ToString());
                        
                        // Create PropertyImage entity with Vercel Blob URL
                        var propertyImage = new PropertyImage
                        {
                            Id = Guid.NewGuid(),
                            PropertyId = property.Id,
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
                
                property.Images = propertyImages;
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
