namespace Auth.Application.DTOs.Response;

// Response DTOs for the 3-step registration process
public class RegisterEmailResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class RegisterVerifyEmailResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
