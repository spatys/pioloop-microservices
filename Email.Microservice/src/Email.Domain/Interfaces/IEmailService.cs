namespace Email.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailVerificationAsync(string to, string confirmationCode);
    Task SendEmailAccountCreatedAsync(string to, string firstName, string lastName);
    Task SendEmailPasswordResetAsync(string to, string resetToken);
    Task SendEmailReservationConfirmationAsync(string to, string reservationDetails);
    Task SendEmailPaymentConfirmationAsync(string to, string paymentDetails);
    Task SendEmailInvoiceAsync(string to, string invoiceNumber, string invoiceUrl);
    Task SendEmailContractAsync(string to, string contractNumber, string contractUrl);
}


