using MediatR;
using Email.Domain.Interfaces;
using Email.Application.DTOs;

namespace Email.Application.Handlers;

public record SendEmailReservationConfirmationCommand(string Email, string ReservationDetails) : IRequest<ApiResponseDto<object>>;

public class SendEmailReservationConfirmationHandler : IRequestHandler<SendEmailReservationConfirmationCommand, ApiResponseDto<object>>
{
    private readonly IEmailService _emailService;

    public SendEmailReservationConfirmationHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<object>> Handle(SendEmailReservationConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ReservationDetails))
            {
                var errors = new
                {
                    email = string.IsNullOrWhiteSpace(request.Email) ? "L'email est requis" : null,
                    reservationDetails = string.IsNullOrWhiteSpace(request.ReservationDetails) ? "Les détails de réservation sont requis" : null
                };

                return ApiResponseDto<object>.ValidationErrorResponse("Email et détails de réservation requis", errors);
            }

            await _emailService.SendEmailReservationConfirmationAsync(request.Email, request.ReservationDetails);
            
            return ApiResponseDto<object>.SuccessResponse(
                "Email de confirmation de réservation envoyé avec succès",
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
                "Erreur lors de l'envoi de l'email de confirmation de réservation",
                new { email = request.Email }
            );
        }
    }
}
