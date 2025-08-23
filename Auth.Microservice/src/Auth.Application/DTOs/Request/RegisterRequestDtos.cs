namespace Auth.Application.DTOs.Request;

// Request DTOs for the 3-step registration process
public record RegisterEmailRequest(string Email);

public record RegisterVerifyEmailRequest(string Email, string Code);

public record RegisterCompleteRequest(string Email, string FirstName, string LastName, string Password, string ConfirmPassword, string? PhoneNumber);
