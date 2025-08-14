using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Handlers;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, ApiResponseDto<object>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RequestPasswordResetCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _httpClient = new HttpClient();
        _configuration = configuration;
    }

    public async Task<ApiResponseDto<object>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation de l'email (remplace les décorateurs)
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

            // Recherche de l'utilisateur
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Pour des raisons de sécurité, on ne révèle pas si l'email existe ou non
                return ApiResponseDto<object>.FromSuccess(null, "Si cet email existe, un lien de réinitialisation a été envoyé");
            }

            // Générer le token de réinitialisation
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Appel Email MS pour envoyer l'email de réinitialisation
            try
            {
                var emailApiUrl = _configuration["EmailApi:BaseUrl"] ?? "http://email-microservice";
                var payload = new
                {
                    Email = user.Email,
                    Token = token,
                    ResetUrl = $"{_configuration["AppUrl"]}/reset-password?token={token}&email={user.Email}"
                };
                using var response = await _httpClient.PostAsJsonAsync($"{emailApiUrl}/api/email/send-password-reset", payload, cancellationToken);
                // ignore response content; best-effort
            }
            catch { /* ignore send failures */ }

            return ApiResponseDto<object>.FromSuccess(null, "Si cet email existe, un lien de réinitialisation a été envoyé");
        }
        catch (Exception ex)
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
