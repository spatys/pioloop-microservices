using MediatR;
using Email.Domain.Interfaces;
using Email.Application.DTOs;

namespace Email.Application.Handlers;

public record SendEmailAccountCreatedCommand(string Email, string FirstName, string LastName) : IRequest<ApiResponseDto<object>>;

public class SendEmailAccountCreatedHandler : IRequestHandler<SendEmailAccountCreatedCommand, ApiResponseDto<object>>
{
    private readonly IEmailService _emailService;

    public SendEmailAccountCreatedHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<object>> Handle(SendEmailAccountCreatedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                var errors = new
                {
                    email = string.IsNullOrWhiteSpace(request.Email) ? "L'email est requis" : null,
                    firstName = string.IsNullOrWhiteSpace(request.FirstName) ? "Le prénom est requis" : null,
                    lastName = string.IsNullOrWhiteSpace(request.LastName) ? "Le nom est requis" : null
                };

                return ApiResponseDto<object>.ValidationErrorResponse("Email, prénom et nom requis", errors);
            }

            await _emailService.SendEmailAccountCreatedAsync(request.Email, request.FirstName, request.LastName);
            
            return ApiResponseDto<object>.SuccessResponse(
                "Email de bienvenue envoyé avec succès",
                new { email = request.Email, firstName = request.FirstName, lastName = request.LastName }
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
                "Erreur lors de l'envoi de l'email de bienvenue",
                new { email = request.Email }
            );
        }
    }
}
