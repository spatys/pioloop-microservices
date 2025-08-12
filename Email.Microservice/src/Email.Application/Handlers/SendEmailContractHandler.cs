using MediatR;
using Email.Domain.Interfaces;
using Email.Application.DTOs;

namespace Email.Application.Handlers;

public record SendEmailContractCommand(string Email, string ContractNumber, string ContractUrl) : IRequest<ApiResponseDto<object>>;

public class SendEmailContractHandler : IRequestHandler<SendEmailContractCommand, ApiResponseDto<object>>
{
    private readonly IEmailService _emailService;

    public SendEmailContractHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<object>> Handle(SendEmailContractCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ContractNumber) || string.IsNullOrWhiteSpace(request.ContractUrl))
            {
                var errors = new
                {
                    email = string.IsNullOrWhiteSpace(request.Email) ? "L'email est requis" : null,
                    contractNumber = string.IsNullOrWhiteSpace(request.ContractNumber) ? "Le numéro de contrat est requis" : null,
                    contractUrl = string.IsNullOrWhiteSpace(request.ContractUrl) ? "L'URL de contrat est requise" : null
                };

                return ApiResponseDto<object>.ValidationErrorResponse("Email, numéro de contrat et URL requis", errors);
            }

            await _emailService.SendEmailContractAsync(request.Email, request.ContractNumber, request.ContractUrl);
            
            return ApiResponseDto<object>.SuccessResponse(
                "Email de contrat envoyé avec succès",
                new { email = request.Email, contractNumber = request.ContractNumber }
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
                "Erreur lors de l'envoi de l'email de contrat",
                new { email = request.Email }
            );
        }
    }
}
