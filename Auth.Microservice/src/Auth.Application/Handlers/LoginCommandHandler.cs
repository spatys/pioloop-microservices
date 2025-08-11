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

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserPasswordRepository userPasswordRepository,
        IPasswordService passwordService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _userPasswordRepository = userPasswordRepository;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation des champs
            if (string.IsNullOrEmpty(request.Email))
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "L'email est requis"
                });
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["password"] = "Le mot de passe est requis"
                });
            }

            // Recherche de l'utilisateur
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Email ou mot de passe incorrect"
                });
            }

            // Vérification du mot de passe
            var userPassword = await _userPasswordRepository.GetByUserIdAsync(user.Id);
            if (userPassword == null || !_passwordService.VerifyPassword(request.Password, userPassword.PasswordHash, userPassword.PasswordSalt))
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["password"] = "Email ou mot de passe incorrect"
                });
            }

            // Vérification que l'utilisateur est actif
            if (!user.IsActive)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Compte désactivé"
                });
            }

            // Mise à jour de la dernière connexion
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);

            // Génération du token JWT
            var token = GenerateJwtToken(user);

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
            return ApiResponseDto<LoginResponseDto>.Error("Erreur interne du serveur");
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "default-secret-key"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.GetFullName()),
            new Claim("email_confirmed", user.EmailConfirmed.ToString()),
            new Claim("consent_accepted", user.ConsentAccepted.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:ExpirationHours"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
