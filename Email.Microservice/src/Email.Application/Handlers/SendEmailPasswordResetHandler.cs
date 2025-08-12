using MediatR;
using Email.Domain.Interfaces;
using Email.Application.DTOs;

namespace Email.Application.Handlers;

public record SendEmailPasswordResetCommand(string Email, string ResetToken) : IRequest<ApiResponseDto<object>>;

public class SendEmailPasswordResetHandler : IRequestHandler<SendEmailPasswordResetCommand, ApiResponseDto<object>>
{
    private readonly IEmailService _emailService;

    public SendEmailPasswordResetHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<object>> Handle(SendEmailPasswordResetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ResetToken))
            {
                var errors = new
                {
                    email = string.IsNullOrWhiteSpace(request.Email) ? "L'email est requis" : null,
                    resetToken = string.IsNullOrWhiteSpace(request.ResetToken) ? "Le token de réinitialisation est requis" : null
                };

                return ApiResponseDto<object>.ValidationErrorResponse("Email et token de réinitialisation requis", errors);
            }

            await _emailService.SendEmailPasswordResetAsync(request.Email, request.ResetToken);
            
            return ApiResponseDto<object>.SuccessResponse(
                "Email de réinitialisation envoyé avec succès",
                new { email = request.Email }
            );
        }
        catch (ArgumentException ex)
        {
            return ApiResponseDto<object>.ErrorResponse(
                ex.Message,
                "ValidationError",
                null,
                new { email = request.Email }
            );
        }
        catch (Exception)
        {
            return ApiResponseDto<object>.InternalErrorResponse(
                "Erreur lors de l'envoi de l'email de réinitialisation",
                new { email = request.Email }
            );
        }
    }
}
