namespace Auth.Application.DTOs;

public class EmailVerificationRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
