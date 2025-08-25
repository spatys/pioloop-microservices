using MediatR;
using Microsoft.AspNetCore.Http;
using Property.Application.Commands;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Domain.Interfaces;
using PropertyEntity = Property.Domain.Entities.Property;
using System.Security.Claims;

namespace Property.Application.Handlers;

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreatePropertyCommandHandler(IPropertyRepository propertyRepository, IHttpContextAccessor httpContextAccessor)
    {
        _propertyRepository = propertyRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PropertyResponse> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
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
            Status = PropertyStatus.Draft,
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
                Type = a.Type,
                Category = a.Category,
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
            property.Images = request.CreatePropertyRequest.Images.Select(i => new PropertyImage
            {
                Id = Guid.NewGuid(),
                PropertyId = property.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsMainImage = i.IsMainImage,
                DisplayOrder = i.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }

        var createdProperty = await _propertyRepository.AddAsync(property);

        return new PropertyResponse
        {
            Id = createdProperty.Id,
            Title = createdProperty.Title,
            Description = createdProperty.Description,
            PropertyType = createdProperty.PropertyType,
            MaxGuests = createdProperty.MaxGuests,
            Bedrooms = createdProperty.Bedrooms,
            Beds = createdProperty.Beds,
            Bathrooms = createdProperty.Bathrooms,
            SquareMeters = createdProperty.SquareMeters,
                            Address = createdProperty.Address,
                Neighborhood = createdProperty.Neighborhood,
                City = createdProperty.City,
                PostalCode = createdProperty.PostalCode,
            Latitude = createdProperty.Latitude,
            Longitude = createdProperty.Longitude,
            PricePerNight = createdProperty.PricePerNight,
            CleaningFee = createdProperty.CleaningFee,
            ServiceFee = createdProperty.ServiceFee,
            Status = createdProperty.Status.ToString(),
            OwnerId = createdProperty.OwnerId,
            CreatedAt = createdProperty.CreatedAt,
            UpdatedAt = createdProperty.UpdatedAt
        };
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
}
