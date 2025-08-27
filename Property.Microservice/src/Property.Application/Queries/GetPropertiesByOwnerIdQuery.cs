using MediatR;
using Property.Application.DTOs.Response;

namespace Property.Application.Queries;

public class GetPropertiesByOwnerIdQuery : IRequest<IEnumerable<PropertyResponse>>
{
    public Guid OwnerId { get; }

    public GetPropertiesByOwnerIdQuery(Guid ownerId)
    {
        OwnerId = ownerId;
    }
}
