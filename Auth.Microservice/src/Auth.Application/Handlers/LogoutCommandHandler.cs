using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace Auth.Application.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponseDto<object>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponseDto<object>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Supprimer le cookie d'authentification
                httpContext.Response.Cookies.Delete("auth_token");
            }

            return ApiResponseDto<object>.FromSuccess(null, "Déconnexion réussie");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<object>.Error("Erreur lors de la déconnexion");
        }
    }
}
