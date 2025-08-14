using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class ChangePasswordCommand : IRequest<ApiResponseDto<object>>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
