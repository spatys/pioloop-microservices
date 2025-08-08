using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Queries;

public class GetUserByEmailQuery : IRequest<ApiResponseDto<UserDto>>
{
    public string Email { get; set; } = string.Empty;
}
