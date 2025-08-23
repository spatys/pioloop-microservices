using Microsoft.Extensions.Configuration;
using Email.Domain.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

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

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Validation basique avec regex
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
                return false;

            // Validation avec MailAddress
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // Validation de l'adresse email
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("L'adresse email de destination ne peut pas être vide.");
            }

            if (!IsValidEmail(to))
            {
                throw new ArgumentException($"L'adresse email '{to}' n'est pas valide. Format attendu: user@domain.com");
            }

            var emailSettings = _configuration.GetSection("EmailSettings");
            var fromEmail = emailSettings["FromEmail"] ?? "noreply@pioloop.com";
            var fromName = emailSettings["FromName"] ?? "Pioloop";

            // Validation de l'adresse email d'expédition
            if (!IsValidEmail(fromEmail))
            {
                throw new ArgumentException($"L'adresse email d'expédition '{fromEmail}' n'est pas valide. Vérifiez la configuration EmailSettings:FromEmail.");
            }

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
        catch (ArgumentException)
        {
            // Re-lancer les ArgumentException telles quelles
            throw;
        }
        catch (Exception ex)
        {
            // Log l'erreur pour le débogage
            Console.WriteLine($"Erreur lors de l'envoi d'email à {to}: {ex.Message}");
            
            // Retourner une erreur plus descriptive
            if (ex.Message.Contains("authentication"))
            {
                throw new InvalidOperationException("Erreur d'authentification SMTP. Vérifiez les paramètres EmailSettings:SmtpUsername et EmailSettings:SmtpPassword.");
            }
            else if (ex.Message.Contains("connection"))
            {
                throw new InvalidOperationException("Erreur de connexion SMTP. Vérifiez les paramètres EmailSettings:SmtpServer et EmailSettings:SmtpPort.");
            }
            else
            {
                throw new InvalidOperationException($"Erreur lors de l'envoi de l'email: {ex.Message}");
            }
        }
    }

    public async Task SendEmailVerificationAsync(string to, string confirmationCode)
    {
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
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Si vous n'avez pas créé de compte avec Pioloop, veuillez ignorer cet email.
                    </p>
                   <p style='color: #999; font-size: 12px; text-align: center;'>L'équipe Pioloop</p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailAccountCreatedAsync(string to, string firstName, string lastName)
    {
        var subject = "Bienvenue chez Pioloop - Votre compte a été créé avec succès !";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>Bienvenue chez Pioloop !</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Votre compte a été créé avec succès</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Bonjour {firstName} {lastName} !</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Nous sommes ravis de vous accueillir dans la communauté Pioloop. Votre compte a été créé avec succès et vous pouvez maintenant profiter de tous nos services.
                    </p>
                    <div style='background: #f8f9fa; border-radius: 8px; padding: 20px; margin: 25px 0;'>
                        <h3 style='color: #667eea; margin-top: 0;'>Prochaines étapes :</h3>
                        <ul style='color: #666; line-height: 1.8;'>
                            <li>Complétez votre profil</li>
                            <li>Explorez nos services</li>
                            <li>Contactez notre support si nécessaire</li>
                        </ul>
                    </div>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Merci de faire confiance à Pioloop pour vos besoins.
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>L'équipe Pioloop</p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailPasswordResetAsync(string to, string resetToken)
    {
        var subject = "Réinitialisation de votre mot de passe - Pioloop";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>Réinitialisation du mot de passe</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Votre demande a été traitée</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Nouveau mot de passe</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Vous avez demandé la réinitialisation de votre mot de passe. Voici votre nouveau mot de passe temporaire :
                    </p>
                    <div style='background: #f8f9fa; border: 2px dashed #dee2e6; border-radius: 8px; padding: 20px; text-align: center; margin: 25px 0;'>
                        <span style='font-size: 24px; font-weight: bold; color: #667eea;'>{resetToken}</span>
                    </div>
                    <p style='color: #666; font-size: 14px; margin-top: 20px;'>
                        <strong>Important :</strong> Changez ce mot de passe dès votre prochaine connexion pour des raisons de sécurité.
                    </p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Si vous n'avez pas demandé cette réinitialisation, contactez immédiatement notre support.
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>L'équipe Pioloop</p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailReservationConfirmationAsync(string to, string reservationDetails)
    {
        var subject = "Confirmation de réservation - Pioloop";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>Réservation confirmée !</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Votre réservation a été validée</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Détails de votre réservation</h2>
                    <div style='background: #f8f9fa; border-radius: 8px; padding: 20px; margin: 25px 0;'>
                        {reservationDetails}
                    </div>
                    <p style='color: #666; line-height: 1.6; margin-top: 25px;'>
                        Merci de votre confiance. Nous vous contacterons bientôt pour confirmer les derniers détails.
                    </p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Pour toute question, n'hésitez pas à nous contacter.
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>L'équipe Pioloop</p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailPaymentConfirmationAsync(string to, string paymentDetails)
    {
        var subject = "Confirmation de paiement - Pioloop";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>Paiement confirmé !</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Votre transaction a été validée</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Détails de votre paiement</h2>
                    <div style='background: #f8f9fa; border-radius: 8px; padding: 20px; margin: 25px 0;'>
                        {paymentDetails}
                    </div>
                    <p style='color: #666; line-height: 1.6; margin-top: 25px;'>
                        Votre paiement a été traité avec succès. Vous recevrez bientôt un reçu détaillé.
                    </p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Merci de votre confiance en Pioloop.
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>L'équipe Pioloop</p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailInvoiceAsync(string to, string invoiceNumber, string invoiceUrl)
    {
        var subject = $"Facture #{invoiceNumber} - Pioloop";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>Votre facture est prête</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Facture #{invoiceNumber}</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Facture #{invoiceNumber}</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Votre facture est maintenant disponible. Cliquez sur le lien ci-dessous pour la consulter et la télécharger.
                    </p>
                    <div style='text-align: center; margin: 25px 0;'>
                        <a href='{invoiceUrl}' style='background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; display: inline-block; font-weight: bold;'>
                            Voir la facture
                        </a>
                    </div>
                    <p style='color: #666; font-size: 14px; margin-top: 20px;'>
                        <strong>Date limite de paiement :</strong> 30 jours à compter de la date d'émission.
                    </p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Pour toute question concernant cette facture, contactez notre service comptabilité.
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>L'équipe Pioloop</p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailContractAsync(string to, string contractNumber, string contractUrl)
    {
        var subject = $"Contrat #{contractNumber} - Pioloop";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>Votre contrat est prêt</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Contrat #{contractNumber}</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Contrat #{contractNumber}</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Votre contrat est maintenant disponible. Cliquez sur le lien ci-dessous pour le consulter et le signer.
                    </p>
                    <div style='text-align: center; margin: 25px 0;'>
                        <a href='{contractUrl}' style='background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; display: inline-block; font-weight: bold;'>
                            Voir le contrat
                        </a>
                    </div>
                    <p style='color: #666; font-size: 14px; margin-top: 20px;'>
                        <strong>Important :</strong> Veuillez signer ce contrat dans les 7 jours pour finaliser votre engagement.
                    </p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Pour toute question concernant ce contrat, contactez notre service juridique.
                    </p>
                    <p style='color: #999; font-size: 12px; text-align: center;'>L'équipe Pioloop</p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }
}


