using Microsoft.Extensions.Configuration;
using Email.Domain.Interfaces;
using System.Net.Mail;
using System.Net;

namespace Email.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        var emailSettings = _configuration.GetSection("EmailSettings");
        _smtpClient = new SmtpClient
        {
            Host = emailSettings["SmtpServer"] ?? "smtp.gmail.com",
            Port = int.Parse(emailSettings["SmtpPort"] ?? "587"),
            EnableSsl = true,
            Credentials = new NetworkCredential(
                emailSettings["SmtpUsername"],
                emailSettings["SmtpPassword"]
            )
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var fromEmail = emailSettings["FromEmail"] ?? "noreply@pioloop.com";
        var fromName = emailSettings["FromName"] ?? "Pioloop";

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await _smtpClient.SendMailAsync(message);
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmationCode)
    {
        var expirationMinutes = _configuration.GetValue<int>("Auth:EmailVerificationExpiration", 10);
        var subject = "Confirmez votre email - Pioloop";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>Bienvenue chez Pioloop !</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Votre code de vérification est prêt</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Vérification de l'email</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Veuillez saisir ce code à 6 chiffres pour vérifier votre adresse email :
                    </p>
                    <div style='background: #f8f9fa; border: 2px dashed #dee2e6; border-radius: 8px; padding: 20px; text-align: center; margin: 25px 0;'>
                        <span style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 8px;'>{confirmationCode}</span>
                    </div>
                    <p style='color: #666; font-size: 14px; margin-top: 20px;'>
                        Ce code expirera dans <strong>{expirationMinutes} minute{(expirationMinutes > 1 ? "s" : "")}</strong>.
                    </p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Si vous n'avez pas créé de compte avec Pioloop, veuillez ignorer cet email.
                    </p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public Task SendAccountCreatedAsync(string to, string firstName, string lastName)
        => SendEmailAsync(to, "Bienvenue chez Pioloop - Votre compte a été créé avec succès !", $"Bonjour {firstName} {lastName}");

    public Task SendPasswordResetAsync(string to, string resetToken)
        => SendEmailAsync(to, "Reset your password - Pioloop", $"Token: {resetToken}");

    public Task SendReservationConfirmationAsync(string to, string reservationDetails)
        => SendEmailAsync(to, "Reservation Confirmed - Pioloop", reservationDetails);

    public Task SendPaymentConfirmationAsync(string to, string paymentDetails)
        => SendEmailAsync(to, "Payment Confirmed - Pioloop", paymentDetails);

    public Task SendInvoiceAsync(string to, string invoiceNumber, string invoiceUrl)
        => SendEmailAsync(to, $"Invoice #{invoiceNumber} - Pioloop", $"<a href='{invoiceUrl}'>View</a>");

    public Task SendContractAsync(string to, string contractNumber, string contractUrl)
        => SendEmailAsync(to, $"Contract #{contractNumber} - Pioloop", $"<a href='{contractUrl}'>View</a>");
}


