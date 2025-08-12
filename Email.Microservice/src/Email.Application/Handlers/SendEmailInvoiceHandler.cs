using MediatR;
using Email.Domain.Interfaces;
using Email.Application.DTOs;

namespace Email.Application.Handlers;

public record SendEmailInvoiceCommand(string Email, string InvoiceNumber, string InvoiceUrl) : IRequest<ApiResponseDto<object>>;

public class SendEmailInvoiceHandler : IRequestHandler<SendEmailInvoiceCommand, ApiResponseDto<object>>
{
    private readonly IEmailService _emailService;

    public SendEmailInvoiceHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<object>> Handle(SendEmailInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.InvoiceNumber) || string.IsNullOrWhiteSpace(request.InvoiceUrl))
            {
                var errors = new
                {
                    email = string.IsNullOrWhiteSpace(request.Email) ? "L'email est requis" : null,
                    invoiceNumber = string.IsNullOrWhiteSpace(request.InvoiceNumber) ? "Le numéro de facture est requis" : null,
                    invoiceUrl = string.IsNullOrWhiteSpace(request.InvoiceUrl) ? "L'URL de facture est requise" : null
                };

                return ApiResponseDto<object>.ValidationErrorResponse("Email, numéro de facture et URL requis", errors);
            }

            await _emailService.SendEmailInvoiceAsync(request.Email, request.InvoiceNumber, request.InvoiceUrl);
            
            return ApiResponseDto<object>.SuccessResponse(
                "Email de facture envoyé avec succès",
                new { email = request.Email, invoiceNumber = request.InvoiceNumber }
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
                "Erreur lors de l'envoi de l'email de facture",
                new { email = request.Email }
            );
        }
    }
}
