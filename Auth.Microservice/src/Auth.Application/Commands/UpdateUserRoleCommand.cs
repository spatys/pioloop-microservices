using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class UpdateUserRoleCommand : IRequest<ApiResponseDto<object>>
{
    public Guid UserId { get; set; }
    public string NewRole { get; set; } = string.Empty;
}
