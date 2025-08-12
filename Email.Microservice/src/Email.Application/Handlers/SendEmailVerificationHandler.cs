using MediatR;
using Email.Domain.Interfaces;
using Email.Application.DTOs;

namespace Email.Application.Handlers;

public record SendEmailVerificationCommand(string Email, string Code) : IRequest<ApiResponseDto<object>>;

public class SendEmailVerificationHandler : IRequestHandler<SendEmailVerificationCommand, ApiResponseDto<object>>
{
    private readonly IEmailService _emailService;

    public SendEmailVerificationHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<object>> Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            {
                var errors = new
                {
                    email = string.IsNullOrWhiteSpace(request.Email) ? "L'email est requis" : null,
                    code = string.IsNullOrWhiteSpace(request.Code) ? "Le code est requis" : null
                };

                return ApiResponseDto<object>.ValidationErrorResponse("Email et code requis", errors);
            }

            await _emailService.SendEmailVerificationAsync(request.Email, request.Code);
            
            return ApiResponseDto<object>.SuccessResponse(
                "Email de vérification envoyé avec succès",
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
                "Erreur lors de l'envoi de l'email de vérification",
                new { email = request.Email }
            );
        }
    }
}
