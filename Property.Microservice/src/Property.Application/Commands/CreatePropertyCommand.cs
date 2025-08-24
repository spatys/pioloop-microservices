using MediatR;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;

namespace Property.Application.Commands;

public class CreatePropertyCommand : IRequest<PropertyResponse>
{
    public CreatePropertyRequest CreatePropertyRequest { get; }

    public CreatePropertyCommand(CreatePropertyRequest createPropertyRequest)
    {
        CreatePropertyRequest = createPropertyRequest;
    }
}
