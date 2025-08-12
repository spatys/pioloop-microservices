using MediatR;
using Email.Domain.Interfaces;
using Email.Application.DTOs;

namespace Email.Application.Handlers;

public record SendEmailPaymentConfirmationCommand(string Email, string PaymentDetails) : IRequest<ApiResponseDto<object>>;

public class SendEmailPaymentConfirmationHandler : IRequestHandler<SendEmailPaymentConfirmationCommand, ApiResponseDto<object>>
{
    private readonly IEmailService _emailService;

    public SendEmailPaymentConfirmationHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<object>> Handle(SendEmailPaymentConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PaymentDetails))
            {
                var errors = new
                {
                    email = string.IsNullOrWhiteSpace(request.Email) ? "L'email est requis" : null,
                    paymentDetails = string.IsNullOrWhiteSpace(request.PaymentDetails) ? "Les détails de paiement sont requis" : null
                };

                return ApiResponseDto<object>.ValidationErrorResponse("Email et détails de paiement requis", errors);
            }

            await _emailService.SendEmailPaymentConfirmationAsync(request.Email, request.PaymentDetails);
            
            return ApiResponseDto<object>.SuccessResponse(
                "Email de confirmation de paiement envoyé avec succès",
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
                "Erreur lors de l'envoi de l'email de confirmation de paiement",
                new { email = request.Email }
            );
        }
    }
}
