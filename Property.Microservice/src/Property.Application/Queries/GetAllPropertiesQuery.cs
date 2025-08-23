using MediatR;
using Property.Application.DTOs;

namespace Property.Application.Queries;

public record GetAllPropertiesQuery : IRequest<IEnumerable<PropertyDto>>;
