using Microsoft.EntityFrameworkCore;
using Property.Domain.Entities;

namespace Property.Infrastructure.Data;

public class PropertyDbContext : DbContext
{
    public PropertyDbContext(DbContextOptions<PropertyDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<PropertyAmenity> PropertyAmenities { get; set; }
    public DbSet<PropertyAvailability> PropertyAvailabilities { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de Property
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.PropertyType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RoomType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CleaningFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ServiceFee).HasColumnType("decimal(18,2)");
            
            // Relations
            entity.HasOne(e => e.Owner)
                  .WithMany(e => e.Properties)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de PropertyImage
        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AltText).HasMaxLength(200);
            
            entity.HasOne(e => e.Property)
                  .WithMany(e => e.Images)
                  .HasForeignKey(e => e.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de PropertyAmenity
        modelBuilder.Entity<PropertyAmenity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.Property)
                  .WithMany(e => e.Amenities)
                  .HasForeignKey(e => e.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de PropertyAvailability
        modelBuilder.Entity<PropertyAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.SpecialPrice).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Property)
                  .WithMany(e => e.Availability)
                  .HasForeignKey(e => e.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });
    }
}
