using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Property.Infrastructure.Data;
using Property.Infrastructure.Services;

namespace Property.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        // 1. Apply migrations first
        var context = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
        await context.Database.MigrateAsync();
        


        // 2. Then seed data
        var seedService = scope.ServiceProvider.GetRequiredService<IDatabaseSeedService>();
        await seedService.SeedAsync();
    }
}
