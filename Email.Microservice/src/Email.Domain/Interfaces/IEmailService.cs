namespace Email.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailConfirmationAsync(string to, string confirmationCode);
    Task SendAccountCreatedAsync(string to, string firstName, string lastName);
    Task SendPasswordResetAsync(string to, string resetToken);
    Task SendReservationConfirmationAsync(string to, string reservationDetails);
    Task SendPaymentConfirmationAsync(string to, string paymentDetails);
    Task SendInvoiceAsync(string to, string invoiceNumber, string invoiceUrl);
    Task SendContractAsync(string to, string contractNumber, string contractUrl);
}


