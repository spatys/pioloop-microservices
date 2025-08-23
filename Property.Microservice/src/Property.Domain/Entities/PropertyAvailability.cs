using System.ComponentModel.DataAnnotations;

namespace Property.Domain.Entities;

public class PropertyAvailability
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid PropertyId { get; set; }
    public virtual Property Property { get; set; } = null!;
    
    [Required]
    public DateTime CheckInDate { get; set; }
    
    [Required]
    public DateTime CheckOutDate { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public decimal? SpecialPrice { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
