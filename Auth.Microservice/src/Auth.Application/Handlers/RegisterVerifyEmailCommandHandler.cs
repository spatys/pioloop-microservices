using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;

namespace Auth.Application.Handlers;

public class RegisterVerifyEmailCommandHandler : IRequestHandler<RegisterVerifyEmailCommand, ApiResponseDto<RegisterVerifyEmailResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterVerifyEmailCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApiResponseDto<RegisterVerifyEmailResponseDto>> Handle(RegisterVerifyEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation des champs
            if (string.IsNullOrEmpty(request.Email))
            {
                return ApiResponseDto<RegisterVerifyEmailResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "L'email est requis"
                });
            }

            if (string.IsNullOrEmpty(request.Code))
            {
                return ApiResponseDto<RegisterVerifyEmailResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["code"] = "Le code de vérification est requis"
                });
            }

            // Recherche de l'utilisateur
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<RegisterVerifyEmailResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Utilisateur non trouvé"
                });
            }

            // Vérification du code
            if (user.IsEmailCodeValid(request.Code))
            {
                // Confirmation de l'email
                user.EmailConfirmed = true;
                user.ResetEmailCodeAttempts();
                await _userManager.UpdateAsync(user);

                var responseDto = new RegisterVerifyEmailResponseDto
                {
                    Message = "Email vérifié avec succès. Vous pouvez maintenant compléter votre inscription.",
                    Email = user.Email,
                    IsVerified = true
                };

                return ApiResponseDto<RegisterVerifyEmailResponseDto>.FromSuccess(responseDto);
            }
            else
            {
                // Incrémentation des tentatives
                user.IncrementEmailCodeAttempts();
                await _userManager.UpdateAsync(user);

                return ApiResponseDto<RegisterVerifyEmailResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["code"] = "Code de vérification invalide"
                });
            }
        }
        catch (Exception ex)
        {
            return ApiResponseDto<RegisterVerifyEmailResponseDto>.Error("Erreur interne du serveur");
        }
    }
}
