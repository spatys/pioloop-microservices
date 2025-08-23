using MediatR;

namespace Property.Application.Commands;

public class DeletePropertyCommand : IRequest<bool>
{
    public Guid Id { get; }

    public DeletePropertyCommand(Guid id)
    {
        Id = id;
    }
}
