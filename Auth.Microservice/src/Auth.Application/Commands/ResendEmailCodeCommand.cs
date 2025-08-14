using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class ResendEmailCodeCommand : IRequest<ApiResponseDto<ResendEmailCodeResponseDto>>
{
    public string Email { get; set; } = string.Empty;
}
