using Microsoft.AspNetCore.Mvc;
using Email.Domain.Interfaces;
using Email.Application.DTOs.Request;

namespace Email.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-email-verification")]
    public async Task<IActionResult> SendEmailVerification([FromBody] SendEmailVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { success = false, message = "Email et code requis" });
        }

        await _emailService.SendEmailVerificationAsync(request.Email, request.Code);
        return Ok(new { success = true });
    }

    [HttpPost("send-email-account-created")]
    public async Task<IActionResult> SendEmailAccountCreated([FromBody] SendEmailAccountCreatedRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { success = false, message = "Email, prénom et nom requis" });
        }

        await _emailService.SendEmailAccountCreatedAsync(request.Email, request.FirstName, request.LastName);
        return Ok(new { success = true });
    }

    [HttpPost("send-email-password-reset")]
    public async Task<IActionResult> SendEmailPasswordReset([FromBody] SendEmailPasswordResetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ResetToken))
        {
            return BadRequest(new { success = false, message = "Email et token de réinitialisation requis" });
        }

        await _emailService.SendEmailPasswordResetAsync(request.Email, request.ResetToken);
        return Ok(new { success = true });
    }

    [HttpPost("send-email-reservation-confirmation")]
    public async Task<IActionResult> SendEmailReservationConfirmation([FromBody] SendEmailReservationConfirmationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ReservationDetails))
        {
            return BadRequest(new { success = false, message = "Email et détails de réservation requis" });
        }

        await _emailService.SendEmailReservationConfirmationAsync(request.Email, request.ReservationDetails);
        return Ok(new { success = true });
    }

    [HttpPost("send-email-payment-confirmation")]
    public async Task<IActionResult> SendEmailPaymentConfirmation([FromBody] SendEmailPaymentConfirmationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PaymentDetails))
        {
            return BadRequest(new { success = false, message = "Email et détails de paiement requis" });
        }

        await _emailService.SendEmailPaymentConfirmationAsync(request.Email, request.PaymentDetails);
        return Ok(new { success = true });
    }

    [HttpPost("send-email-invoice")]
    public async Task<IActionResult> SendEmailInvoice([FromBody] SendEmailInvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.InvoiceNumber) || string.IsNullOrWhiteSpace(request.InvoiceUrl))
        {
            return BadRequest(new { success = false, message = "Email, numéro de facture et URL requis" });
        }

        await _emailService.SendEmailInvoiceAsync(request.Email, request.InvoiceNumber, request.InvoiceUrl);
        return Ok(new { success = true });
    }

    [HttpPost("send-email-contract")]
    public async Task<IActionResult> SendEmailContract([FromBody] SendEmailContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ContractNumber) || string.IsNullOrWhiteSpace(request.ContractUrl))
        {
            return BadRequest(new { success = false, message = "Email, numéro de contrat et URL requis" });
        }

        await _emailService.SendEmailContractAsync(request.Email, request.ContractNumber, request.ContractUrl);
        return Ok(new { success = true });
    }
}


