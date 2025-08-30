using System.ComponentModel.DataAnnotations;
using Property.Domain.Enums;

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
    public AmenityType Type { get; set; }
    
    [Required]
    public AmenityCategory Category { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public bool IsIncludedInRent { get; set; } = true;
    
    public decimal? AdditionalCost { get; set; }
    
    [MaxLength(50)]
    public string Icon { get; set; } = string.Empty;
    
    public int Priority { get; set; } = 1;
    
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
