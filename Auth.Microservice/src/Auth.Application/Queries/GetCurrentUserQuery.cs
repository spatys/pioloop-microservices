using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Queries;

public class GetCurrentUserQuery : IRequest<ApiResponseDto<ApplicationUserDto>>
{
    // Pas de paramètres - l'utilisateur est extrait du token JWT
}
