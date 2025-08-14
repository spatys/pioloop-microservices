using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class ResetPasswordCommand : IRequest<ApiResponseDto<object>>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
