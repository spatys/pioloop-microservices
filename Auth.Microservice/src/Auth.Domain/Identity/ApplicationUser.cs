using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? LastLoginAt { get; set; }

    // Email verification
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailCodeExpiry { get; set; }
    public int EmailCodeAttempts { get; set; }

    public string GetFullName() => string.Join(' ', new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

    public void GenerateEmailVerificationCode()
    {
        var random = new Random();
        EmailVerificationCode = random.Next(100000, 999999).ToString();
        EmailCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        EmailCodeAttempts = 0;
    }

    public bool IsEmailCodeValid(string code)
    {
        if (string.IsNullOrWhiteSpace(EmailVerificationCode) || EmailCodeExpiry is null)
            return false;
        if (DateTime.UtcNow > EmailCodeExpiry)
            return false;
        return EmailVerificationCode == code;
    }

    public void IncrementEmailCodeAttempts() => EmailCodeAttempts++;
    public void ResetEmailCodeAttempts() => EmailCodeAttempts = 0;
    public void ClearEmailVerificationState()
    {
        EmailVerificationCode = null;
        EmailCodeExpiry = null;
        EmailCodeAttempts = 0;
    }
}


