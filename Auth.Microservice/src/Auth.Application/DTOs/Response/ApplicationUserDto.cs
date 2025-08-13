namespace Auth.Application.DTOs.Response;

public class ApplicationUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool ConsentAccepted { get; set; }
    public DateTime? ConsentAcceptedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}


