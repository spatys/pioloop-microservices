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

    public async Task SendEmailVerificationAsync(string to, string confirmationCode)
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

    public async Task SendEmailAccountCreatedAsync(string to, string firstName, string lastName)
    {
        var subject = "Bienvenue chez Pioloop - Votre compte a été créé avec succès !";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='margin: 0; font-size: 28px;'>🎉 Bienvenue chez Pioloop !</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Votre compte a été créé avec succès</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Bonjour {firstName} {lastName},</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Nous sommes ravis de vous accueillir dans la communauté Pioloop ! Votre compte a été créé avec succès et vous pouvez dès maintenant profiter de tous nos services.
                    </p>
                    
                    <div style='background: #f8f9fa; border-left: 4px solid #667eea; padding: 20px; margin: 25px 0; border-radius: 0 8px 8px 0;'>
                        <h3 style='color: #333; margin: 0 0 15px 0;'>🚀 Que pouvez-vous faire maintenant ?</h3>
                        <ul style='color: #666; margin: 0; padding-left: 20px;'>
                            <li style='margin-bottom: 8px;'>Explorer nos propriétés disponibles</li>
                            <li style='margin-bottom: 8px;'>Créer votre profil propriétaire</li>
                            <li style='margin-bottom: 8px;'>Gérer vos réservations</li>
                            <li style='margin-bottom: 8px;'>Accéder à votre tableau de bord</li>
                        </ul>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='http://localhost:3000' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Accéder à mon compte
                        </a>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Merci de faire confiance à Pioloop pour vos besoins immobiliers.
                    </p>
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
                    <h1 style='margin: 0; font-size: 28px;'>🔐 Réinitialisation du mot de passe</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Sécurisez votre compte</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Demande de réinitialisation</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Nous avons reçu une demande de réinitialisation de mot de passe pour votre compte Pioloop. Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer cet email.
                    </p>
                    
                    <div style='background: #f8f9fa; border: 2px dashed #dee2e6; border-radius: 8px; padding: 20px; text-align: center; margin: 25px 0;'>
                        <p style='color: #666; margin: 0 0 15px 0; font-weight: bold;'>Votre token de réinitialisation :</p>
                        <span style='font-size: 24px; font-weight: bold; color: #667eea; letter-spacing: 4px;'>{resetToken}</span>
                    </div>
                    
                    <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 15px; margin: 25px 0;'>
                        <p style='color: #856404; margin: 0; font-size: 14px;'>
                            <strong>⚠️ Important :</strong> Ce token expirera dans 1 heure pour des raisons de sécurité.
                        </p>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='http://localhost:3000/reset-password?token={resetToken}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Réinitialiser mon mot de passe
                        </a>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Si vous n'avez pas demandé cette réinitialisation, veuillez ignorer cet email.
                    </p>
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
                    <h1 style='margin: 0; font-size: 28px;'>✅ Réservation confirmée</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Votre séjour est confirmé</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Félicitations !</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Votre réservation a été confirmée avec succès. Nous vous remercions de votre confiance et nous nous réjouissons de vous accueillir.
                    </p>
                    
                    <div style='background: #f8f9fa; border-radius: 8px; padding: 25px; margin: 25px 0;'>
                        <h3 style='color: #333; margin: 0 0 15px 0;'>📋 Détails de votre réservation</h3>
                        <div style='color: #666; line-height: 1.8;'>
                            {reservationDetails}
                        </div>
                    </div>
                    
                    <div style='background: #d4edda; border: 1px solid #c3e6cb; border-radius: 8px; padding: 15px; margin: 25px 0;'>
                        <p style='color: #155724; margin: 0; font-size: 14px;'>
                            <strong>💡 Prochaines étapes :</strong> Vous recevrez un email de rappel 24h avant votre arrivée avec toutes les informations pratiques.
                        </p>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='http://localhost:3000/dashboard/reservations' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Voir mes réservations
                        </a>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Pour toute question, n'hésitez pas à nous contacter via votre tableau de bord.
                    </p>
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
                    <h1 style='margin: 0; font-size: 28px;'>💳 Paiement confirmé</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Transaction sécurisée effectuée</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Paiement traité avec succès</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Nous confirmons que votre paiement a été traité avec succès. Merci de votre confiance.
                    </p>
                    
                    <div style='background: #f8f9fa; border-radius: 8px; padding: 25px; margin: 25px 0;'>
                        <h3 style='color: #333; margin: 0 0 15px 0;'>📊 Détails de la transaction</h3>
                        <div style='color: #666; line-height: 1.8;'>
                            {paymentDetails}
                        </div>
                    </div>
                    
                    <div style='background: #d1ecf1; border: 1px solid #bee5eb; border-radius: 8px; padding: 15px; margin: 25px 0;'>
                        <p style='color: #0c5460; margin: 0; font-size: 14px;'>
                            <strong>🔒 Sécurité :</strong> Votre paiement a été traité de manière sécurisée et vos informations sont protégées.
                        </p>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='http://localhost:3000/dashboard/payments' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Voir mes transactions
                        </a>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Ce reçu fait foi de votre transaction. Conservez-le précieusement.
                    </p>
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
                    <h1 style='margin: 0; font-size: 28px;'>📄 Facture disponible</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Facture #{invoiceNumber}</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Votre facture est prête</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Votre facture #{invoiceNumber} est maintenant disponible. Vous pouvez la télécharger ou la consulter en ligne.
                    </p>
                    
                    <div style='background: #f8f9fa; border-radius: 8px; padding: 25px; margin: 25px 0;'>
                        <h3 style='color: #333; margin: 0 0 15px 0;'>📋 Informations de facturation</h3>
                        <ul style='color: #666; margin: 0; padding-left: 20px;'>
                            <li style='margin-bottom: 8px;'>Numéro de facture : <strong>{invoiceNumber}</strong></li>
                            <li style='margin-bottom: 8px;'>Date d'émission : <strong>{DateTime.Now:dd/MM/yyyy}</strong></li>
                            <li style='margin-bottom: 8px;'>Statut : <strong style='color: #28a745;'>Payée</strong></li>
                        </ul>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{invoiceUrl}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Télécharger la facture
                        </a>
                    </div>
                    
                    <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 15px; margin: 25px 0;'>
                        <p style='color: #856404; margin: 0; font-size: 14px;'>
                            <strong>💡 Conseil :</strong> Conservez cette facture pour vos déclarations fiscales.
                        </p>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Pour toute question concernant cette facture, contactez notre service client.
                    </p>
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
                    <h1 style='margin: 0; font-size: 28px;'>📋 Contrat disponible</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Contrat #{contractNumber}</p>
                </div>
                <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #333; margin-bottom: 20px;'>Votre contrat est prêt</h2>
                    <p style='color: #666; line-height: 1.6; margin-bottom: 25px;'>
                        Votre contrat #{contractNumber} est maintenant disponible. Veuillez le consulter attentivement avant de le signer.
                    </p>
                    
                    <div style='background: #f8f9fa; border-radius: 8px; padding: 25px; margin: 25px 0;'>
                        <h3 style='color: #333; margin: 0 0 15px 0;'>📋 Informations du contrat</h3>
                        <ul style='color: #666; margin: 0; padding-left: 20px;'>
                            <li style='margin-bottom: 8px;'>Numéro de contrat : <strong>{contractNumber}</strong></li>
                            <li style='margin-bottom: 8px;'>Date de création : <strong>{DateTime.Now:dd/MM/yyyy}</strong></li>
                            <li style='margin-bottom: 8px;'>Statut : <strong style='color: #ffc107;'>En attente de signature</strong></li>
                        </ul>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{contractUrl}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Consulter le contrat
                        </a>
                    </div>
                    
                    <div style='background: #d1ecf1; border: 1px solid #bee5eb; border-radius: 8px; padding: 15px; margin: 25px 0;'>
                        <p style='color: #0c5460; margin: 0; font-size: 14px;'>
                            <strong>⚖️ Important :</strong> Ce contrat est légalement contraignant. Lisez-le attentivement avant de le signer.
                        </p>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='color: #999; font-size: 12px; text-align: center;'>
                        Pour toute question concernant ce contrat, contactez notre service juridique.
                    </p>
                </div>
            </div>";

        await SendEmailAsync(to, subject, body);
    }
}


