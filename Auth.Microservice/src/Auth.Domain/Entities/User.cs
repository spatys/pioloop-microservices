using System.ComponentModel.DataAnnotations;

namespace Auth.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    
    // Email verification properties
    public string? EmailVerificationCode { get; private set; }
    public DateTime? EmailCodeExpiry { get; private set; }
    public int EmailCodeAttempts { get; private set; }
    public DateTime? EmailCodeBlockedUntil { get; private set; }
    
    // Consent properties
    public bool ConsentAccepted { get; private set; }
    public DateTime? ConsentAcceptedAt { get; private set; }
    public string? ConsentIpAddress { get; private set; }
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { } // For EF Core

    public User(string email, string firstName, string lastName)
    {
        Id = Guid.NewGuid();
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        EmailConfirmed = false;
        EmailCodeAttempts = 0;
        ConsentAccepted = false;
    }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}".Trim();
    }

    // Domain methods
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    // Consent methods
    public void AcceptConsent(string? ipAddress = null)
    {
        ConsentAccepted = true;
        ConsentAcceptedAt = DateTime.UtcNow;
        ConsentIpAddress = ipAddress;
    }

    public void RevokeConsent()
    {
        ConsentAccepted = false;
        ConsentAcceptedAt = null;
        ConsentIpAddress = null;
    }

    public bool HasValidConsent()
    {
        return ConsentAccepted && ConsentAcceptedAt.HasValue;
    }

    // Email verification methods
    public void GenerateEmailVerificationCode(int expirationMinutes = 10)
    {
        var random = new Random();
        EmailVerificationCode = random.Next(100000, 999999).ToString();
        EmailCodeExpiry = DateTime.UtcNow.AddMinutes(expirationMinutes);
        EmailCodeAttempts = 0;
        EmailCodeBlockedUntil = null;
    }

    public bool IsEmailCodeValid(string code)
    {
        if (EmailCodeBlockedUntil.HasValue && DateTime.UtcNow < EmailCodeBlockedUntil.Value)
        {
            return false; // Temporarily blocked
        }

        if (EmailCodeExpiry.HasValue && DateTime.UtcNow > EmailCodeExpiry.Value)
        {
            return false; // Code expired
        }

        return EmailVerificationCode == code;
    }

    public void IncrementEmailCodeAttempts()
    {
        EmailCodeAttempts++;
        
        // Block after 5 failed attempts
        if (EmailCodeAttempts >= 5)
        {
            EmailCodeBlockedUntil = DateTime.UtcNow.AddMinutes(30); // Block for 30 minutes
        }
    }

    public void ResetEmailCodeAttempts()
    {
        EmailCodeAttempts = 0;
        EmailCodeBlockedUntil = null;
    }

    public bool CanRequestEmailCode()
    {
        return !EmailCodeBlockedUntil.HasValue || DateTime.UtcNow >= EmailCodeBlockedUntil.Value;
    }

    public void ClearEmailVerificationState()
    {
        EmailVerificationCode = null;
        EmailCodeExpiry = null;
        EmailCodeAttempts = 0;
        EmailCodeBlockedUntil = null;
    }
}
