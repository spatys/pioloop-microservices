namespace Email.Application.DTOs.Request;

// Request DTOs for Email API
public record SendEmailVerificationRequest(string Email, string Code);

public record SendEmailAccountCreatedRequest(string Email, string FirstName, string LastName);

public record SendEmailPasswordResetRequest(string Email, string ResetToken);

public record SendEmailReservationConfirmationRequest(string Email, string ReservationDetails);

public record SendEmailPaymentConfirmationRequest(string Email, string PaymentDetails);

public record SendEmailInvoiceRequest(string Email, string InvoiceNumber, string InvoiceUrl);

public record SendEmailContractRequest(string Email, string ContractNumber, string ContractUrl);
