using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class RegisterEmailCommand : IRequest<ApiResponseDto<RegisterEmailResponseDto>>
{
    public string Email { get; set; } = string.Empty;
}
