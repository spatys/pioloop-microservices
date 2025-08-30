using MediatR;
using Auth.Application.Queries;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Auth.Application.Handlers;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponseDto<ApplicationUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetCurrentUserQueryHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponseDto<ApplicationUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // L'utilisateur est déjà authentifié via le cookie HttpOnly
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return ApiResponseDto<ApplicationUserDto>.Error("Utilisateur non authentifié");
            }

            // Extraire l'ID utilisateur depuis les claims
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return ApiResponseDto<ApplicationUserDto>.Error("Token d'authentification invalide");
            }

            // Récupérer l'utilisateur
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return ApiResponseDto<ApplicationUserDto>.Error("Utilisateur non trouvé");
            }

            // Récupérer les rôles de l'utilisateur
            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new ApplicationUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            };

            return ApiResponseDto<ApplicationUserDto>.FromSuccess(userDto);
        }
        catch (Exception)
        {
            return ApiResponseDto<ApplicationUserDto>.Error("Erreur interne du serveur");
        }
    }
}
