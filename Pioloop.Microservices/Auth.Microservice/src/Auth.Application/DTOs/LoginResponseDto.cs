namespace Auth.Application.DTOs;

public class LoginResponseDto
{
    public string Message { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
