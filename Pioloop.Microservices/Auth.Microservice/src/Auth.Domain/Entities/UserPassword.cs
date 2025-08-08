namespace Auth.Domain.Entities;

public class UserPassword
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastChangedAt { get; private set; }
    public bool IsActive { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    private UserPassword() { } // For EF Core

    public UserPassword(Guid userId, string passwordHash, string passwordSalt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdatePassword(string newPasswordHash, string newPasswordSalt)
    {
        PasswordHash = newPasswordHash;
        PasswordSalt = newPasswordSalt;
        LastChangedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
