using MediatR;
using Property.Application.Queries;
using Property.Application.DTOs.Response;
using Property.Domain.Interfaces;
using AutoMapper;

namespace Property.Application.Handlers;

public class GetPropertyAvailabilityQueryHandler : IRequestHandler<GetPropertyAvailabilityQuery, List<PropertyAvailabilityResponse>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public GetPropertyAvailabilityQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<List<PropertyAvailabilityResponse>> Handle(GetPropertyAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var availabilities = await _propertyRepository.GetPropertyAvailabilitiesAsync(
            request.Request.PropertyId,
            request.Request.StartDate,
            request.Request.EndDate
        );

        return _mapper.Map<List<PropertyAvailabilityResponse>>(availabilities);
    }
}

public class GetPropertyAvailabilityCalendarQueryHandler : IRequestHandler<GetPropertyAvailabilityCalendarQuery, PropertyAvailabilityCalendarResponse>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetPropertyAvailabilityCalendarQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyAvailabilityCalendarResponse> Handle(GetPropertyAvailabilityCalendarQuery request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
        if (property == null)
        {
            throw new ArgumentException("Property not found");
        }

        var startDate = request.StartDate ?? DateTime.Today;
        var endDate = request.EndDate ?? DateTime.Today.AddMonths(3);

        var availabilities = await _propertyRepository.GetPropertyAvailabilitiesAsync(
            request.PropertyId,
            startDate,
            endDate
        );

        var calendar = new List<AvailabilityDay>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var isToday = currentDate.Date == DateTime.Today;
            var isPast = currentDate.Date < DateTime.Today;

            // Trouver la disponibilité pour cette date
            var availability = availabilities.FirstOrDefault(a => 
                currentDate >= a.CheckInDate && currentDate < a.CheckOutDate);

            var day = new AvailabilityDay
            {
                Date = currentDate,
                IsAvailable = availability?.IsAvailable ?? true,
                Price = availability?.SpecialPrice ?? property.PricePerNight,
                Notes = availability?.Notes,
                IsToday = isToday,
                IsPast = isPast,
                IsSpecialPrice = availability?.SpecialPrice.HasValue == true
            };

            calendar.Add(day);
            currentDate = currentDate.AddDays(1);
        }

        return new PropertyAvailabilityCalendarResponse
        {
            PropertyId = request.PropertyId,
            Calendar = calendar,
            BasePrice = property.PricePerNight,
            Currency = "XAF"
        };
    }
}
