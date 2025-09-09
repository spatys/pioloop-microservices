using MediatR;
using Property.Application.Commands;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using Property.Domain.Interfaces;
using AutoMapper;

namespace Property.Application.Handlers;

public class CreatePropertyAvailabilityCommandHandler : IRequestHandler<CreatePropertyAvailabilityCommand, PropertyAvailabilityResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public CreatePropertyAvailabilityCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<PropertyAvailabilityResponse> Handle(CreatePropertyAvailabilityCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que la propriété existe
        var property = await _propertyRepository.GetByIdAsync(request.Request.PropertyId);
        if (property == null)
        {
            throw new ArgumentException("Property not found");
        }

        // Vérifier les conflits de dates
        var hasConflict = await _propertyRepository.HasAvailabilityConflictAsync(
            request.Request.PropertyId,
            request.Request.CheckInDate,
            request.Request.CheckOutDate,
            null
        );

        if (hasConflict)
        {
            throw new InvalidOperationException("Date conflict with existing availability");
        }

        var availability = new PropertyAvailability
        {
            Id = Guid.NewGuid(),
            PropertyId = request.Request.PropertyId,
            CheckInDate = request.Request.CheckInDate,
            CheckOutDate = request.Request.CheckOutDate,
            IsAvailable = request.Request.IsAvailable,
            SpecialPrice = request.Request.SpecialPrice,
            Notes = request.Request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _propertyRepository.AddAvailabilityAsync(availability);
        await _propertyRepository.SaveChangesAsync();

        return _mapper.Map<PropertyAvailabilityResponse>(availability);
    }
}

public class UpdatePropertyAvailabilityCommandHandler : IRequestHandler<UpdatePropertyAvailabilityCommand, PropertyAvailabilityResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public UpdatePropertyAvailabilityCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<PropertyAvailabilityResponse> Handle(UpdatePropertyAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var availability = await _propertyRepository.GetAvailabilityByIdAsync(request.Request.Id);
        if (availability == null)
        {
            throw new ArgumentException("Availability not found");
        }

        // Vérifier les conflits de dates (exclure l'élément actuel)
        var hasConflict = await _propertyRepository.HasAvailabilityConflictAsync(
            availability.PropertyId,
            request.Request.CheckInDate,
            request.Request.CheckOutDate,
            request.Request.Id
        );

        if (hasConflict)
        {
            throw new InvalidOperationException("Date conflict with existing availability");
        }

        availability.CheckInDate = request.Request.CheckInDate;
        availability.CheckOutDate = request.Request.CheckOutDate;
        availability.IsAvailable = request.Request.IsAvailable;
        availability.SpecialPrice = request.Request.SpecialPrice;
        availability.Notes = request.Request.Notes;
        availability.UpdatedAt = DateTime.UtcNow;

        await _propertyRepository.UpdateAvailabilityAsync(availability);
        await _propertyRepository.SaveChangesAsync();

        return _mapper.Map<PropertyAvailabilityResponse>(availability);
    }
}

public class DeletePropertyAvailabilityCommandHandler : IRequestHandler<DeletePropertyAvailabilityCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;

    public DeletePropertyAvailabilityCommandHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<bool> Handle(DeletePropertyAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var availability = await _propertyRepository.GetAvailabilityByIdAsync(request.Id);
        if (availability == null)
        {
            return false;
        }

        await _propertyRepository.DeleteAvailabilityAsync(request.Id);
        await _propertyRepository.SaveChangesAsync();

        return true;
    }
}

public class BulkUpdateAvailabilityCommandHandler : IRequestHandler<BulkUpdateAvailabilityCommand, List<PropertyAvailabilityResponse>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public BulkUpdateAvailabilityCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<List<PropertyAvailabilityResponse>> Handle(BulkUpdateAvailabilityCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que la propriété existe
        var property = await _propertyRepository.GetByIdAsync(request.Request.PropertyId);
        if (property == null)
        {
            throw new ArgumentException("Property not found");
        }

        var results = new List<PropertyAvailabilityResponse>();

        foreach (var period in request.Request.Periods)
        {
            // Vérifier les conflits pour chaque période
            var hasConflict = await _propertyRepository.HasAvailabilityConflictAsync(
                request.Request.PropertyId,
                period.StartDate,
                period.EndDate,
                null
            );

            if (!hasConflict)
            {
                var availability = new PropertyAvailability
                {
                    Id = Guid.NewGuid(),
                    PropertyId = request.Request.PropertyId,
                    CheckInDate = period.StartDate,
                    CheckOutDate = period.EndDate,
                    IsAvailable = period.IsAvailable,
                    SpecialPrice = period.SpecialPrice,
                    Notes = period.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _propertyRepository.AddAvailabilityAsync(availability);
                results.Add(_mapper.Map<PropertyAvailabilityResponse>(availability));
            }
        }

        await _propertyRepository.SaveChangesAsync();
        return results;
    }
}
