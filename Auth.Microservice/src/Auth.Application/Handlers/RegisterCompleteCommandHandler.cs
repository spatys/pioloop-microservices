using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Auth.Domain.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Services;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Handlers;

public class RegisterCompleteCommandHandler : IRequestHandler<RegisterCompleteCommand, ApiResponseDto<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordRepository _userPasswordRepository;
    private readonly IPasswordService _passwordService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RegisterCompleteCommandHandler(
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

    public async Task<ApiResponseDto<UserDto>> Handle(RegisterCompleteCommand request, CancellationToken cancellationToken)
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

            if (!request.AcceptConsent)
            {
                fieldErrors["acceptConsent"] = "Vous devez accepter les conditions d'utilisation";
            }

            if (fieldErrors.Count > 0)
            {
                return ApiResponseDto<UserDto>.ValidationError(fieldErrors);
            }

            // Recherche de l'utilisateur existant
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<UserDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Utilisateur non trouvé"
                });
            }

            // Vérification que l'email est confirmé
            if (!user.EmailConfirmed)
            {
                return ApiResponseDto<UserDto>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "L'email doit être vérifié avant de compléter l'inscription"
                });
            }

            // Mise à jour du profil utilisateur
            user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
            
            if (request.AcceptConsent)
            {
                user.AcceptConsent();
            }

            // Mise à jour de l'utilisateur
            var updatedUser = await _userRepository.UpdateAsync(user);

            // Hashage et sauvegarde du mot de passe
            var passwordHash = _passwordService.HashPassword(request.Password, out var passwordSalt);
            var userPassword = new UserPassword(user.Id, passwordHash, passwordSalt);
            await _userPasswordRepository.CreateAsync(userPassword);

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
            var userDto = new UserDto
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
                ConsentAccepted = updatedUser.ConsentAccepted,
                ConsentAcceptedAt = updatedUser.ConsentAcceptedAt,
                Roles = new List<string>()
            };

            return ApiResponseDto<UserDto>.FromSuccess(userDto, "Inscription complétée avec succès");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<UserDto>.Error("Erreur interne du serveur");
        }
    }
}
