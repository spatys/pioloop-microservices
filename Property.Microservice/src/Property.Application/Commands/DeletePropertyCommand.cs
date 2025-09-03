using MediatR;
using Property.Application.DTOs.Response;

namespace Property.Application.Commands;

public class DeletePropertyCommand : IRequest<PropertyResponse>
{
    public Guid Id { get; }

    public DeletePropertyCommand(Guid id)
    {
        Id = id;
    }
}
