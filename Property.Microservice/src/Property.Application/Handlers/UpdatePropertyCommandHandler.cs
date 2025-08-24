using MediatR;
using Property.Application.Commands;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Domain.Interfaces;

namespace Property.Application.Handlers;

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;

    public UpdatePropertyCommandHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
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
        existingProperty.RoomType = request.UpdatePropertyRequest.RoomType;
        existingProperty.MaxGuests = request.UpdatePropertyRequest.MaxGuests;
        existingProperty.Bedrooms = request.UpdatePropertyRequest.Bedrooms;
        existingProperty.Beds = request.UpdatePropertyRequest.Beds;
        existingProperty.Bathrooms = request.UpdatePropertyRequest.Bathrooms;
        existingProperty.Address = request.UpdatePropertyRequest.Address;
        existingProperty.City = request.UpdatePropertyRequest.City;
        existingProperty.PostalCode = request.UpdatePropertyRequest.PostalCode;
        existingProperty.Latitude = request.UpdatePropertyRequest.Latitude ?? 0;
        existingProperty.Longitude = request.UpdatePropertyRequest.Longitude ?? 0;
        existingProperty.PricePerNight = request.UpdatePropertyRequest.PricePerNight;
        existingProperty.CleaningFee = request.UpdatePropertyRequest.CleaningFee;
        existingProperty.ServiceFee = request.UpdatePropertyRequest.ServiceFee;
        existingProperty.IsInstantBookable = request.UpdatePropertyRequest.IsInstantBookable;
        existingProperty.UpdatedAt = DateTime.UtcNow;

        var updatedProperty = await _propertyRepository.UpdateAsync(existingProperty);

        return new PropertyResponse
        {
            Id = updatedProperty.Id,
            Title = updatedProperty.Title,
            Description = updatedProperty.Description,
            PropertyType = updatedProperty.PropertyType,
            RoomType = updatedProperty.RoomType,
            MaxGuests = updatedProperty.MaxGuests,
            Bedrooms = updatedProperty.Bedrooms,
            Beds = updatedProperty.Beds,
            Bathrooms = updatedProperty.Bathrooms,
            Address = updatedProperty.Address,
            City = updatedProperty.City,
            PostalCode = updatedProperty.PostalCode,
            Latitude = updatedProperty.Latitude,
            Longitude = updatedProperty.Longitude,
            PricePerNight = updatedProperty.PricePerNight,
            CleaningFee = updatedProperty.CleaningFee,
            ServiceFee = updatedProperty.ServiceFee,
            IsInstantBookable = updatedProperty.IsInstantBookable,
            Status = updatedProperty.Status.ToString(),
            OwnerId = updatedProperty.OwnerId,
            OwnerName = string.Empty, // À récupérer depuis Auth.Microservice si nécessaire
            OwnerEmail = string.Empty, // À récupérer depuis Auth.Microservice si nécessaire
            ImageUrls = updatedProperty.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
            Amenities = updatedProperty.Amenities.OrderBy(a => a.DisplayOrder).Select(a => a.Name).ToList(),
            CreatedAt = updatedProperty.CreatedAt,
            UpdatedAt = updatedProperty.UpdatedAt
        };
    }
}
