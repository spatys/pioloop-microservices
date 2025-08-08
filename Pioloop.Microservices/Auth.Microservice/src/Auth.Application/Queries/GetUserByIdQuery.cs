using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Queries;

public class GetUserByIdQuery : IRequest<ApiResponseDto<UserDto>>
{
    public Guid Id { get; set; }
}
