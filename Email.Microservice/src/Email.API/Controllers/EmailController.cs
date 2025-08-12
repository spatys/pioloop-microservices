using Microsoft.AspNetCore.Mvc;
using MediatR;
using Email.Application.DTOs;
using Email.Application.DTOs.Request;
using Email.Application.Handlers;

namespace Email.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
[ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
[ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
public class EmailController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Envoie un email de vérification avec un code de confirmation
    /// </summary>
    /// <param name="request">Données de la demande d'envoi d'email de vérification</param>
    /// <returns>Résultat de l'envoi de l'email</returns>
    [HttpPost("send-email-verification")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
    public async Task<IActionResult> SendEmailVerification([FromBody] SendEmailVerificationRequest request)
    {
        var command = new SendEmailVerificationCommand(request.Email, request.Code);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        if (result.ErrorType == "ValidationError")
        {
            return BadRequest(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Envoie un email de bienvenue lors de la création d'un compte
    /// </summary>
    /// <param name="request">Données de la demande d'envoi d'email de bienvenue</param>
    /// <returns>Résultat de l'envoi de l'email</returns>
    [HttpPost("send-email-account-created")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
    public async Task<IActionResult> SendEmailAccountCreated([FromBody] SendEmailAccountCreatedRequest request)
    {
        var command = new SendEmailAccountCreatedCommand(request.Email, request.FirstName, request.LastName);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        if (result.ErrorType == "ValidationError")
        {
            return BadRequest(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Envoie un email de réinitialisation de mot de passe
    /// </summary>
    /// <param name="request">Données de la demande d'envoi d'email de réinitialisation</param>
    /// <returns>Résultat de l'envoi de l'email</returns>
    [HttpPost("send-email-password-reset")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
    public async Task<IActionResult> SendEmailPasswordReset([FromBody] SendEmailPasswordResetRequest request)
    {
        var command = new SendEmailPasswordResetCommand(request.Email, request.ResetToken);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        if (result.ErrorType == "ValidationError")
        {
            return BadRequest(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Envoie un email de confirmation de réservation
    /// </summary>
    /// <param name="request">Données de la demande d'envoi d'email de confirmation de réservation</param>
    /// <returns>Résultat de l'envoi de l'email</returns>
    [HttpPost("send-email-reservation-confirmation")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
    public async Task<IActionResult> SendEmailReservationConfirmation([FromBody] SendEmailReservationConfirmationRequest request)
    {
        var command = new SendEmailReservationConfirmationCommand(request.Email, request.ReservationDetails);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        if (result.ErrorType == "ValidationError")
        {
            return BadRequest(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Envoie un email de confirmation de paiement
    /// </summary>
    /// <param name="request">Données de la demande d'envoi d'email de confirmation de paiement</param>
    /// <returns>Résultat de l'envoi de l'email</returns>
    [HttpPost("send-email-payment-confirmation")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
    public async Task<IActionResult> SendEmailPaymentConfirmation([FromBody] SendEmailPaymentConfirmationRequest request)
    {
        var command = new SendEmailPaymentConfirmationCommand(request.Email, request.PaymentDetails);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        if (result.ErrorType == "ValidationError")
        {
            return BadRequest(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Envoie un email de facture
    /// </summary>
    /// <param name="request">Données de la demande d'envoi d'email de facture</param>
    /// <returns>Résultat de l'envoi de l'email</returns>
    [HttpPost("send-email-invoice")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
    public async Task<IActionResult> SendEmailInvoice([FromBody] SendEmailInvoiceRequest request)
    {
        var command = new SendEmailInvoiceCommand(request.Email, request.InvoiceNumber, request.InvoiceUrl);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        if (result.ErrorType == "ValidationError")
        {
            return BadRequest(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Envoie un email de contrat
    /// </summary>
    /// <param name="request">Données de la demande d'envoi d'email de contrat</param>
    /// <returns>Résultat de l'envoi de l'email</returns>
    [HttpPost("send-email-contract")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 500)]
    public async Task<IActionResult> SendEmailContract([FromBody] SendEmailContractRequest request)
    {
        var command = new SendEmailContractCommand(request.Email, request.ContractNumber, request.ContractUrl);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        if (result.ErrorType == "ValidationError")
        {
            return BadRequest(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Endpoint de santé pour vérifier que le service fonctionne
    /// </summary>
    /// <returns>Statut du service</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    public IActionResult Health()
    {
        return Ok(ApiResponseDto<object>.SuccessResponse("Email Microservice is healthy", new { status = "healthy", timestamp = DateTime.UtcNow }));
    }


}


