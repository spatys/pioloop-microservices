using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Auth.Domain.Services;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using Auth.Application.Utils;

namespace Auth.Application.Handlers;

public class RegisterCompleteCommandHandler : IRequestHandler<RegisterCompleteCommand, ApiResponseDto<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;

    public RegisterCompleteCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IJwtService jwtService,
        HttpClient httpClient)
    {
        _userManager = userManager;
        _httpClient = httpClient;
        _configuration = configuration;
        _jwtService = jwtService;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> Handle(RegisterCompleteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation des champs
            var fieldErrors = new Dictionary<string, string>();
            
            if (string.IsNullOrEmpty(request.Email))
            {
                fieldErrors["email"] = "L'email est requis";
            }
            
            if (string.IsNullOrEmpty(request.FirstName))
            {
                fieldErrors["firstName"] = "Le prénom est requis";
            }
            
            if (string.IsNullOrEmpty(request.LastName))
            {
                fieldErrors["lastName"] = "Le nom est requis";
            }
            
            if (string.IsNullOrEmpty(request.Password))
            {
                fieldErrors["password"] = "Le mot de passe est requis";
            }
            else if (request.Password.Length < 8)
            {
                fieldErrors["password"] = "Le mot de passe doit contenir au moins 8 caractères";
            }
            
            if (request.Password != request.ConfirmPassword)
            {
                fieldErrors["confirmPassword"] = "Les mots de passe ne correspondent pas";
            }



            if (fieldErrors.Count > 0)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(fieldErrors);
            }

            // Recherche de l'utilisateur existant
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Utilisateur non trouvé"
                });
            }

            // Vérification que l'email est confirmé
            if (!user.EmailConfirmed)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "L'email doit être vérifié avant de compléter l'inscription"
                });
            }

            // Mise à jour du profil utilisateur
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            // Mise à jour de l'utilisateur
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var mappedErrors = UserManagerErrorMapper.MapUserErrors(updateResult.Errors);
                return ApiResponseDto<LoginResponseDto>.ValidationError(mappedErrors);
            }
            var updatedUser = user;

            // Hashage et sauvegarde du mot de passe
            var pwdResult = await _userManager.AddPasswordAsync(user, request.Password);
            if (!pwdResult.Succeeded)
            {
                var mappedErrors = UserManagerErrorMapper.MapPasswordErrors(pwdResult.Errors);
                return ApiResponseDto<LoginResponseDto>.ValidationError(mappedErrors);
            }

            // Ajouter rôle par défaut Tenant
            if (!await _userManager.IsInRoleAsync(user, "Tenant"))
            {
                await _userManager.AddToRoleAsync(user, "Tenant");
            }

            // Envoi de l'email de bienvenue
            try
            {
                var emailApiUrl = _configuration["EmailApi:BaseUrl"] ?? "http://localhost:5002";
                var payload = new
                {
                    Email = updatedUser.Email,
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName
                };
                using var response = await _httpClient.PostAsJsonAsync($"{emailApiUrl}/api/email/send-email-account-created", payload, cancellationToken);
                // ignore response content; best-effort
            }
            catch { /* ignore send failures */ }

            // Création du DTO utilisateur
            var userDto = new ApplicationUserDto
            {
                Id = updatedUser.Id,
                Email = updatedUser.Email,
                FirstName = updatedUser.FirstName,
                LastName = updatedUser.LastName,
                FullName = updatedUser.GetFullName(),
                PhoneNumber = updatedUser.PhoneNumber,
                IsActive = updatedUser.IsActive,
                CreatedAt = updatedUser.CreatedAt,
                LastLoginAt = updatedUser.LastLoginAt,
                EmailConfirmed = updatedUser.EmailConfirmed,
                Roles = new List<string>()
            };

            // Ajouter les rôles au DTO et générer le token via service
            var roles = (await _userManager.GetRolesAsync(updatedUser)).ToList();
            userDto.Roles = roles;

            // Générer le token et renvoyer LoginResponseDto (le contrôleur posera le cookie et videra le token)
            var token = _jwtService.GenerateToken(updatedUser.Id, updatedUser.Email!, updatedUser.GetFullName(), roles, new Dictionary<string,string>
            {
                ["EmailConfirmed"] = updatedUser.EmailConfirmed.ToString(),
                ["IsActive"] = updatedUser.IsActive.ToString()
            });
            var loginResponse = new LoginResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:ExpirationHours"] ?? "24"))
            };

            return ApiResponseDto<LoginResponseDto>.FromSuccess(loginResponse);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<LoginResponseDto>.Error(ex.Message);
        }
    }
}
