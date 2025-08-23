using MediatR;
using Property.Application.Commands;
using Property.Domain.Interfaces;

namespace Property.Application.Handlers;

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;

    public DeletePropertyCommandHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<bool> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        return await _propertyRepository.DeleteAsync(request.Id);
    }
}
