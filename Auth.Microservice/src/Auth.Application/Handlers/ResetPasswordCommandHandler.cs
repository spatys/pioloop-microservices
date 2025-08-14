using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using Auth.Application.Utils;

namespace Auth.Application.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponseDto<object>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApiResponseDto<object>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation des champs (remplace les décorateurs)
            if (string.IsNullOrEmpty(request.Email))
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "L'email est requis"
                });
            }

            if (!IsValidEmail(request.Email))
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Format d'email invalide"
                });
            }

            if (string.IsNullOrEmpty(request.Token))
            {
                return ApiResponseDto<object>.ValidationError(new Dictionary<string, string>
                {
                    ["token"] = "Le token de réinitialisation est requis"
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

            // Recherche de l'utilisateur
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<object>.Error("Token de réinitialisation invalide");
            }

            // Réinitialiser le mot de passe
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var mappedErrors = UserManagerErrorMapper.MapPasswordErrors(result.Errors);
                return ApiResponseDto<object>.ValidationError(mappedErrors);
            }

            return ApiResponseDto<object>.FromSuccess(null, "Mot de passe réinitialisé avec succès");
        }
        catch (Exception)
        {
            return ApiResponseDto<object>.Error("Erreur interne du serveur");
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
