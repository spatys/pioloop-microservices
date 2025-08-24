using MediatR;
using Property.Application.DTOs.Response;

namespace Property.Application.Queries;

public class GetPropertyByIdQuery : IRequest<PropertyResponse?>
{
    public Guid Id { get; }

    public GetPropertyByIdQuery(Guid id)
    {
        Id = id;
    }
}
