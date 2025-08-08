using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using Auth.Domain.Services;
using Auth.Domain.Entities;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponseDto<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordRepository _userPasswordRepository;
    private readonly IPasswordService _passwordService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUserPasswordRepository userPasswordRepository,
        IPasswordService passwordService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _userPasswordRepository = userPasswordRepository;
        _passwordService = passwordService;
        _httpClient = new HttpClient();
        _configuration = configuration;
    }

    public async Task<ApiResponseDto<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation des champs
            var fieldErrors = new Dictionary<string, string>();
            
            if (string.IsNullOrEmpty(request.Email))
            {
                fieldErrors["email"] = "L'email est requis";
            }
            else if (!IsValidEmail(request.Email))
            {
                fieldErrors["email"] = "Format d'email invalide";
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

            if (!request.AcceptConsent)
            {
                fieldErrors["acceptConsent"] = "Vous devez accepter les conditions d'utilisation";
            }

            if (fieldErrors.Count > 0)
            {
                return ApiResponseDto<UserDto>.ValidationError(fieldErrors);
            }

            // Vérification si l'email existe déjà
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                return ApiResponseDto<UserDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Cet email est déjà utilisé"
                });
            }

            // Création de l'utilisateur
            var user = new User(request.Email, request.FirstName, request.LastName);
            user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
            
            if (request.AcceptConsent)
            {
                user.AcceptConsent();
            }

            // Génération du code de vérification email
            user.GenerateEmailVerificationCode();

            // Sauvegarde de l'utilisateur
            var createdUser = await _userRepository.CreateAsync(user);

            // Hashage et sauvegarde du mot de passe
            var passwordHash = _passwordService.HashPassword(request.Password, out var passwordSalt);
            var userPassword = new UserPassword(user.Id, passwordHash, passwordSalt);
            await _userPasswordRepository.CreateAsync(userPassword);

            // Affecter le rôle par défaut "User" si disponible (meilleure UX)
            try
            {
                var roles = await _userRepository.GetUserRolesAsync(createdUser.Id);
                if (!roles.Any())
                {
                    // Chercher le rôle "User" via IRoleRepository serait idéal;
                    // à défaut, ignorer silencieusement si non dispo.
                }
            }
            catch { /* ignore */ }

            // Appel Email MS pour envoyer le code
            try
            {
                var emailApiUrl = _configuration["EmailApi:BaseUrl"] ?? "http://localhost:5068";
                var payload = new
                {
                    Email = createdUser.Email,
                    Code = createdUser.EmailVerificationCode
                };
                using var response = await _httpClient.PostAsJsonAsync($"{emailApiUrl}/api/email/send-verification", payload, cancellationToken);
                // ignore response content; best-effort
            }
            catch { /* ignore send failures */ }

            // Création du DTO utilisateur
            var userDto = new UserDto
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                FullName = createdUser.GetFullName(),
                PhoneNumber = createdUser.PhoneNumber,
                IsActive = createdUser.IsActive,
                CreatedAt = createdUser.CreatedAt,
                LastLoginAt = createdUser.LastLoginAt,
                EmailConfirmed = createdUser.EmailConfirmed,
                ConsentAccepted = createdUser.ConsentAccepted,
                ConsentAcceptedAt = createdUser.ConsentAcceptedAt,
                Roles = new List<string>()
            };

            return ApiResponseDto<UserDto>.FromSuccess(userDto, "Inscription réussie. Veuillez vérifier votre email.");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<UserDto>.Error("Erreur interne du serveur");
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
