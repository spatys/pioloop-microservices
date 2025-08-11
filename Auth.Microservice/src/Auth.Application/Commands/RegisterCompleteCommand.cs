using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Commands;

public class RegisterCompleteCommand : IRequest<ApiResponseDto<UserDto>>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool AcceptConsent { get; set; }
}
