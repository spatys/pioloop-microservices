using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Handlers;

public class RegisterEmailCommandHandler : IRequestHandler<RegisterEmailCommand, ApiResponseDto<RegisterEmailResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RegisterEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _httpClient = new HttpClient();
        _configuration = configuration;
    }

    public async Task<ApiResponseDto<RegisterEmailResponseDto>> Handle(RegisterEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation de l'email
            if (string.IsNullOrEmpty(request.Email))
            {
                return ApiResponseDto<RegisterEmailResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "L'email est requis"
                });
            }

            if (!IsValidEmail(request.Email))
            {
                return ApiResponseDto<RegisterEmailResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Format d'email invalide"
                });
            }

            // Vérification si l'email existe déjà
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
            {
                return ApiResponseDto<RegisterEmailResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Cet email est déjà utilisé"
                });
            }

            // Création d'un utilisateur temporaire avec email uniquement
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            user.GenerateEmailVerificationCode();

            // Sauvegarde de l'utilisateur temporaire
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return ApiResponseDto<RegisterEmailResponseDto>.Error("Echec de création d'utilisateur");
            }
            var createdUser = user;

            // Appel Email MS pour envoyer le code
            try
            {
                var emailApiUrl = _configuration["EmailApi:BaseUrl"] ?? "http://email-microservice";
                var payload = new
                {
                    Email = createdUser.Email,
                    Code = createdUser.EmailVerificationCode
                };
                using var response = await _httpClient.PostAsJsonAsync($"{emailApiUrl}/api/email/send-email-verification", payload, cancellationToken);
                // ignore response content; best-effort
            }
            catch { /* ignore send failures */ }

            var expirationMinutes = _configuration.GetValue<int>("Auth:EmailVerificationExpiration", 10);
            var responseDto = new RegisterEmailResponseDto
            {
                Message = "Code de vérification envoyé avec succès",
                Email = createdUser.Email,
                ExpirationMinutes = expirationMinutes
            };

            return ApiResponseDto<RegisterEmailResponseDto>.FromSuccess(responseDto, "Code de vérification envoyé avec succès");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<RegisterEmailResponseDto>.Error("Erreur interne du serveur");
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
