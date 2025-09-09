using MediatR;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;

namespace Property.Application.Queries;

public class GetPropertyAvailabilityQuery : IRequest<List<PropertyAvailabilityResponse>>
{
    public GetAvailabilityRequest Request { get; set; } = new();
}

public class GetPropertyAvailabilityCalendarQuery : IRequest<PropertyAvailabilityCalendarResponse>
{
    public Guid PropertyId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
