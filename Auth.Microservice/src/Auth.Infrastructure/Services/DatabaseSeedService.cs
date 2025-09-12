using Microsoft.AspNetCore.Identity;
using Auth.Infrastructure.Data;
using Auth.Domain.Identity;

namespace Auth.Infrastructure.Services;

public interface IDatabaseSeedService
{
    Task SeedAsync();
}

public class DatabaseSeedService : IDatabaseSeedService
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DatabaseSeedService(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    private async Task SeedRolesAsync()
    {
        var requiredRoles = new[] { "Tenant", "Owner", "Manager", "Admin" };
        
        foreach (var role in requiredRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    public async Task SeedAsync()
    {
        // Seed roles
        await SeedRolesAsync();
    }
}
