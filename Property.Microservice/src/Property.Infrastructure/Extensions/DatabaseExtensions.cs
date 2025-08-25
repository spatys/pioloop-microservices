using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Property.Infrastructure.Data;

namespace Property.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        // Apply migrations
        var context = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
        await context.Database.MigrateAsync();
    }
}
