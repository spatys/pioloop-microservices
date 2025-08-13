namespace Auth.Application.DTOs.Response;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public ApplicationUserDto User { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}
