using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Auth.Domain.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Auth.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponseDto<LoginResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordRepository _userPasswordRepository;
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserPasswordRepository userPasswordRepository,
        IPasswordService passwordService,
        IConfiguration configuration,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _userPasswordRepository = userPasswordRepository;
        _passwordService = passwordService;
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

            // Recherche de l'utilisateur par email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Email inexistant => message spécifique
                errors["email"] = "Email incorrect";

                // On retourne ici, car on ne peut pas vérifier un mot de passe sans utilisateur
                return ApiResponseDto<LoginResponseDto>.ValidationError(errors);
            }

            // Vérification du mot de passe
            var userPassword = await _userPasswordRepository.GetByUserIdAsync(user.Id);
            var isPasswordValid = userPassword != null && _passwordService.VerifyPassword(request.Password, userPassword.PasswordHash, userPassword.PasswordSalt);
            if (!isPasswordValid)
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
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);

            // Récupération des rôles et génération du token JWT via service
            var roles = (await _userRepository.GetUserRolesAsync(user.Id)).ToList();
            var token = _jwtService.GenerateToken(user, roles);

            // Création du DTO utilisateur
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmailConfirmed = user.EmailConfirmed,
                ConsentAccepted = user.ConsentAccepted,
                ConsentAcceptedAt = user.ConsentAcceptedAt,
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
