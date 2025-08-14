using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using System.Security.Claims;

namespace Auth.Application.Handlers;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponseDto<object>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponseDto<object>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation des champs (remplace les décorateurs)
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["currentPassword"] = "Le mot de passe actuel est requis"
                });
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["newPassword"] = "Le nouveau mot de passe est requis"
                });
            }

            if (string.IsNullOrEmpty(request.ConfirmNewPassword))
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["confirmNewPassword"] = "La confirmation du mot de passe est requise"
                });
            }

            if (request.NewPassword.Length < 8)
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["newPassword"] = "Le mot de passe doit contenir au moins 8 caractères"
                });
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["confirmNewPassword"] = "Les mots de passe ne correspondent pas"
                });
            }

            // Récupérer l'utilisateur connecté
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return ApiResponseDto<object>.Error("Utilisateur non authentifié");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return ApiResponseDto<object>.Error("Utilisateur non trouvé");
            }

            // Vérifier le mot de passe actuel
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["currentPassword"] = "Le mot de passe actuel est incorrect"
                });
            }

            // Changer le mot de passe
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["newPassword"] = string.Join(", ", errors)
                });
            }

            return ApiResponseDto<object>.FromSuccess(null, "Mot de passe changé avec succès");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<object>.Error("Erreur interne du serveur");
        }
    }
}
