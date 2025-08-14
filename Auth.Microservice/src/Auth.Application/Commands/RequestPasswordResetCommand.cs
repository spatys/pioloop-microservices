using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class RequestPasswordResetCommand : IRequest<ApiResponseDto<object>>
{
    public string Email { get; set; } = string.Empty;
}
