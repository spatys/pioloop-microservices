using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class RegisterVerifyEmailCommand : IRequest<ApiResponseDto<RegisterVerifyEmailResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
