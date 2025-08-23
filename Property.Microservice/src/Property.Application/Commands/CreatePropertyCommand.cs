using MediatR;
using Property.Application.DTOs.Request;

namespace Property.Application.Commands;

public class CreatePropertyCommand : IRequest<PropertyResponse>
{
    public CreatePropertyRequest CreatePropertyRequest { get; }

    public CreatePropertyCommand(CreatePropertyRequest createPropertyRequest)
    {
        CreatePropertyRequest = createPropertyRequest;
    }
}
