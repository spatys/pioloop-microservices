using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Commands;

public class VerifyEmailCommand : IRequest<ApiResponseDto<bool>>
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
