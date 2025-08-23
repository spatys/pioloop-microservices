using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Auth.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;

namespace Auth.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponseDto<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _jwtService = jwtService;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var errors = new Dictionary<string, string>();

            // Validation simple des champs requis
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                errors["email"] = "L'email est requis";
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                errors["password"] = "Le mot de passe est requis";
            }

            if (errors.Count > 0)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(errors);
            }

            // Recherche de l'utilisateur par email via Identity
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Email inexistant => message spécifique
                errors["email"] = "Email incorrect";

                // On retourne ici, car on ne peut pas vérifier un mot de passe sans utilisateur
                return ApiResponseDto<LoginResponseDto>.ValidationError(errors);
            }

            // Vérification du mot de passe via SignInManager
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                errors["password"] = "Mot de passe incorrect";
            }

            // Vérification que l'utilisateur est actif
            if (!user.IsActive)
            {
                errors["email"] = "Compte désactivé";
            }

            if (errors.Count > 0)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(errors);
            }

            // Mise à jour de la dernière connexion
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Récupération des rôles et génération du token JWT via service
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var token = _jwtService.GenerateToken(user.Id, user.Email!, user.GetFullName(), roles, new Dictionary<string,string>
            {
                ["EmailConfirmed"] = user.EmailConfirmed.ToString(),
                ["IsActive"] = user.IsActive.ToString()
            });

            // Création du DTO utilisateur
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
                Roles = new List<string>() // TODO: Ajouter les rôles
            };

            var loginResponse = new LoginResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:ExpirationHours"]))
            };

            return ApiResponseDto<LoginResponseDto>.FromSuccess(loginResponse, "Connexion réussie");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<LoginResponseDto>.Error(ex.Message);
        }
    }

    // removed local token generation; now uses IJwtService
}
