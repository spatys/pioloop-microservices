using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class LogoutCommand : IRequest<ApiResponseDto<object>>
{
    // Pas de paramètres - la déconnexion se fait via le cookie
}
