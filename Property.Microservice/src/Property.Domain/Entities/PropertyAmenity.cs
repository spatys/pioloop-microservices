using System.ComponentModel.DataAnnotations;

namespace Property.Domain.Entities;

public class PropertyAmenity
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid PropertyId { get; set; }
    public virtual Property Property { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Essential, Comfort, Luxury, etc.
    
    public bool IsAvailable { get; set; } = true;
    
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
