namespace Auth.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { } // For EF Core

    public Role(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
