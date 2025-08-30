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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IImageService _imageService;

    public CreatePropertyCommandHandler(
        IPropertyRepository propertyRepository, 
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        HttpClient httpClient,
        IMapper mapper,
        IImageService imageService)
    {
        _propertyRepository = propertyRepository;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _httpClient = httpClient;
        _mapper = mapper;
        _imageService = imageService;
    }

    public async Task<PropertyResponse> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Récupérer l'utilisateur connecté depuis le token JWT
            var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Utilisateur non authentifié");
        }

        if (!Guid.TryParse(userId, out var ownerId))
        {
            throw new InvalidOperationException("Invalid user ID format");
        }

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
            var imageTasks = request.CreatePropertyRequest.Images.Select(async (img, index) =>
            {
                // Convertir base64 en stream
                var imageBytes = Convert.FromBase64String(img.ImageUrl);
                var imageStream = new MemoryStream(imageBytes);
                
                // Upload vers Vercel Blob
                var fileName = $"image_{index + 1}.jpg";
                var imageUrl = await _imageService.UploadImageAsync(imageStream, fileName, property.Id.ToString());
                
                return new PropertyImage
                {
                    Id = Guid.NewGuid(),
                    PropertyId = property.Id,
                    ImageUrl = imageUrl,
                    AltText = img.AltText,
                    IsMainImage = img.IsMainImage,
                    DisplayOrder = img.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
            });

            property.Images = (await Task.WhenAll(imageTasks)).ToList();
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

    private string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
            return userIdClaim?.Value;
        }
        return null;
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
