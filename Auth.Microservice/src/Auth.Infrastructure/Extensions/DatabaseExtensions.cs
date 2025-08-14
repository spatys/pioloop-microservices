using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Services;

namespace Auth.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        // 1. Apply migrations first
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await context.Database.MigrateAsync();
        
        // 2. Then seed data
        var seedService = scope.ServiceProvider.GetRequiredService<IDatabaseSeedService>();
        await seedService.SeedAsync();
    }
}
