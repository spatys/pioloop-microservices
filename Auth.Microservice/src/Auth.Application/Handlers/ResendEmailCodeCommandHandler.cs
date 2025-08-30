using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Handlers;

public class ResendEmailCodeCommandHandler : IRequestHandler<ResendEmailCodeCommand, ApiResponseDto<ResendEmailCodeResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ResendEmailCodeCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _httpClient = new HttpClient();
        _configuration = configuration;
    }

    public async Task<ApiResponseDto<ResendEmailCodeResponseDto>> Handle(ResendEmailCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation de l'email
            if (string.IsNullOrEmpty(request.Email))
            {
                return ApiResponseDto<ResendEmailCodeResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "L'email est requis"
                });
            }

            if (!IsValidEmail(request.Email))
            {
                return ApiResponseDto<ResendEmailCodeResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Format d'email invalide"
                });
            }

            // Recherche de l'utilisateur
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<ResendEmailCodeResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Utilisateur non trouvé"
                });
            }

            // Vérification si l'email est déjà confirmé
            if (user.EmailConfirmed)
            {
                return ApiResponseDto<ResendEmailCodeResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Cet email est déjà vérifié"
                });
            }

            // Vérification du nombre de tentatives (limite à 3 renvois par 1/4 heure)
            // if (user.EmailCodeAttempts >= 3)
            // {
            //     return ApiResponseDto<ResendEmailCodeResponseDto>.ValidationError(new Dictionary<string, string>
            //     {
            //         ["email"] = "Trop de tentatives. Veuillez réessayer dans 15 minutes"
            //     });
            // }

            // Génération d'un nouveau code
            user.GenerateEmailVerificationCode();
            user.ResetEmailCodeAttempts();

            // Sauvegarde de l'utilisateur
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ApiResponseDto<ResendEmailCodeResponseDto>.Error("Echec de mise à jour de l'utilisateur");
            }

            // Appel Email MS pour envoyer le nouveau code
            try
            {
                var emailApiUrl = _configuration["EmailApi:BaseUrl"]; // ?? "http://email-api";
                var payload = new
                {
                    Email = user.Email,
                    Code = user.EmailVerificationCode
                };
                using var response = await _httpClient.PostAsJsonAsync($"{emailApiUrl}/api/email/send-email-verification", payload, cancellationToken);
                // ignore response content; best-effort
            }
            catch { /* ignore send failures */ }

            var responseDto = new ResendEmailCodeResponseDto
            {
                Message = "Nouveau code de vérification envoyé avec succès",
                Email = user.Email
            };

            return ApiResponseDto<ResendEmailCodeResponseDto>.FromSuccess(responseDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ResendEmailCodeResponseDto>.Error("Erreur interne du serveur");
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
