using MediatR;
using Property.Application.Commands;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Domain.Interfaces;

namespace Property.Application.Handlers;

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;

    public CreatePropertyCommandHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyResponse> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = request.CreatePropertyRequest.Title,
            Description = request.CreatePropertyRequest.Description,
            PropertyType = request.CreatePropertyRequest.PropertyType,
            RoomType = request.CreatePropertyRequest.RoomType,
            MaxGuests = request.CreatePropertyRequest.MaxGuests,
            Bedrooms = request.CreatePropertyRequest.Bedrooms,
            Beds = request.CreatePropertyRequest.Beds,
            Bathrooms = request.CreatePropertyRequest.Bathrooms,
            Address = request.CreatePropertyRequest.Address,
            City = request.CreatePropertyRequest.City,
            PostalCode = request.CreatePropertyRequest.PostalCode,
            Latitude = request.CreatePropertyRequest.Latitude,
            Longitude = request.CreatePropertyRequest.Longitude,
            PricePerNight = request.CreatePropertyRequest.PricePerNight,
            CleaningFee = request.CreatePropertyRequest.CleaningFee,
            ServiceFee = request.CreatePropertyRequest.ServiceFee,
            IsInstantBookable = request.CreatePropertyRequest.IsInstantBookable,
            Status = PropertyStatus.Draft,
            OwnerId = request.CreatePropertyRequest.OwnerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdProperty = await _propertyRepository.AddAsync(property);

        return new PropertyResponse
        {
            Id = createdProperty.Id,
            Title = createdProperty.Title,
            Description = createdProperty.Description,
            PropertyType = createdProperty.PropertyType,
            RoomType = createdProperty.RoomType,
            MaxGuests = createdProperty.MaxGuests,
            Bedrooms = createdProperty.Bedrooms,
            Beds = createdProperty.Beds,
            Bathrooms = createdProperty.Bathrooms,
            Address = createdProperty.Address,
            City = createdProperty.City,
            PostalCode = createdProperty.PostalCode,
            Latitude = createdProperty.Latitude,
            Longitude = createdProperty.Longitude,
            PricePerNight = createdProperty.PricePerNight,
            CleaningFee = createdProperty.CleaningFee,
            ServiceFee = createdProperty.ServiceFee,
            IsInstantBookable = createdProperty.IsInstantBookable,
            Status = createdProperty.Status.ToString(),
            OwnerId = createdProperty.OwnerId,
            CreatedAt = createdProperty.CreatedAt,
            UpdatedAt = createdProperty.UpdatedAt
        };
    }
}
