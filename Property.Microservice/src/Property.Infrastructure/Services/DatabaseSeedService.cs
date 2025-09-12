using Property.Infrastructure.Data;
using Property.Infrastructure.Data.SeedData;

namespace Property.Infrastructure.Services;

public interface IDatabaseSeedService
{
    Task SeedAsync();
}

public class DatabaseSeedService : IDatabaseSeedService
{
    private readonly PropertyDbContext _context;

    public DatabaseSeedService(PropertyDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    
    {
        // Seed amenities
        await AmenitySeedData.SeedAmenitiesAsync(_context);
    }
}
