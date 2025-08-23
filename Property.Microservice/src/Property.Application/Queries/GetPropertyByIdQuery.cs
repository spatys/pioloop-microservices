using MediatR;
using Property.Application.DTOs;

namespace Property.Application.Queries;

public class GetPropertyByIdQuery : IRequest<PropertyDto?>
{
    public Guid Id { get; }

    public GetPropertyByIdQuery(Guid id)
    {
        Id = id;
    }
}
