using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Commands;

public class LoginCommand : IRequest<ApiResponseDto<LoginResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
