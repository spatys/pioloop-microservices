using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using Auth.Domain.Services;
using Auth.Domain.Entities;

namespace Auth.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponseDto<LoginResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordRepository _userPasswordRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserPasswordRepository userPasswordRepository,
        IRoleRepository roleRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _userPasswordRepository = userPasswordRepository;
        _roleRepository = roleRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation des champs
            var fieldErrors = new Dictionary<string, string>();
            
            if (string.IsNullOrEmpty(request.Email))
            {
                fieldErrors["email"] = "L'email est requis";
            }
            
            if (string.IsNullOrEmpty(request.Password))
            {
                fieldErrors["password"] = "Le mot de passe est requis";
            }

            if (fieldErrors.Count > 0)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(fieldErrors);
            }

            // Recherche de l'utilisateur
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Email non trouvé"
                });
            }

            if (!user.IsActive)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Compte non actif"
                });
            }

            if (!user.EmailConfirmed)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Email non confirmé. Veuillez vérifier votre email d'abord."
                });
            }

            // Vérification du mot de passe
            var userPassword = await _userPasswordRepository.GetByUserIdAsync(user.Id);
            if (userPassword == null)
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Compte utilisateur non complètement configuré. Veuillez terminer l'inscription d'abord."
                });
            }

            if (!_passwordService.VerifyPassword(request.Password, userPassword.PasswordHash, userPassword.PasswordSalt))
            {
                return ApiResponseDto<LoginResponseDto>.ValidationError(new Dictionary<string, string>
                {
                    ["password"] = "Mot de passe incorrect"
                });
            }

            // Mise à jour de la dernière connexion
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);

            // Récupération des rôles
            var roles = await _userRepository.GetUserRolesAsync(user.Id);

            // Génération du token JWT
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
                Roles = roles
            };

            var loginData = new LoginResponseDto
            {
                Message = "Connexion réussie",
                User = userDto,
                Token = token
            };

            return ApiResponseDto<LoginResponseDto>.FromSuccess(loginData, "Connexion réussie");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<LoginResponseDto>.Error("Erreur interne du serveur");
        }
    }
}
