namespace Property.Domain.Entities;

public class PropertyAmenity
{
    public Guid PropertyId { get; set; }
    public int AmenityId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Property Property { get; set; } = null!;
    public virtual Amenity Amenity { get; set; } = null!;
}