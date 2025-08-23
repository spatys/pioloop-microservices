namespace Auth.Application.DTOs.Response;

public class ResendEmailCodeResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
