using MediatR;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;

namespace Property.Application.Commands;

public class CreatePropertyAvailabilityCommand : IRequest<PropertyAvailabilityResponse>
{
    public CreatePropertyAvailabilityRequest Request { get; set; } = new();
}

public class UpdatePropertyAvailabilityCommand : IRequest<PropertyAvailabilityResponse>
{
    public UpdatePropertyAvailabilityRequest Request { get; set; } = new();
}

public class DeletePropertyAvailabilityCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class BulkUpdateAvailabilityCommand : IRequest<List<PropertyAvailabilityResponse>>
{
    public BulkUpdateAvailabilityRequest Request { get; set; } = new();
}
