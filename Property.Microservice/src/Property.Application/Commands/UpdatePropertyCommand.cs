using MediatR;
using Property.Application.DTOs.Request;

namespace Property.Application.Commands;

public class UpdatePropertyCommand : IRequest<PropertyResponse>
{
    public Guid Id { get; }
    public UpdatePropertyRequest UpdatePropertyRequest { get; }

    public UpdatePropertyCommand(Guid id, UpdatePropertyRequest updatePropertyRequest)
    {
        Id = id;
        UpdatePropertyRequest = updatePropertyRequest;
    }
}
