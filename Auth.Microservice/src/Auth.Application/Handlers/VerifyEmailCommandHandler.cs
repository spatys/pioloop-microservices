using MediatR;
using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;

namespace Auth.Application.Handlers;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, ApiResponseDto<bool>>
{
    private readonly IUserRepository _userRepository;

    public VerifyEmailCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponseDto<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var fieldErrors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                fieldErrors["email"] = "L'email est requis";
            }
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                fieldErrors["code"] = "Le code est requis";
            }
            if (fieldErrors.Count > 0)
            {
                return ApiResponseDto<bool>.ValidationError(fieldErrors);
            }

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<bool>.ValidationError(new Dictionary<string, string>
                {
                    ["email"] = "Utilisateur non trouvé"
                });
            }

            if (user.EmailConfirmed)
            {
                return ApiResponseDto<bool>.FromSuccess(true, "Email déjà confirmé");
            }

            if (!user.IsEmailCodeValid(request.Code))
            {
                user.IncrementEmailCodeAttempts();
                await _userRepository.UpdateAsync(user);
                return ApiResponseDto<bool>.ValidationError(new Dictionary<string, string>
                {
                    ["code"] = "Code invalide ou expiré"
                });
            }

            user.ConfirmEmail();
            user.ClearEmailVerificationState();
            await _userRepository.UpdateAsync(user);

            return ApiResponseDto<bool>.FromSuccess(true, "Email confirmé avec succès");
        }
        catch
        {
            return ApiResponseDto<bool>.Error("Erreur interne du serveur");
        }
    }
}


