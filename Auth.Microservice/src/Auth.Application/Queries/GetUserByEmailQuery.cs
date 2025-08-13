using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Queries;

public class GetUserByEmailQuery : IRequest<ApiResponseDto<ApplicationUserDto>>
{
    public string Email { get; set; } = string.Empty;
}
