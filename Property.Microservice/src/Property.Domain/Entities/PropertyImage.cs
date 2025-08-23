using System.ComponentModel.DataAnnotations;

namespace Property.Domain.Entities;

public class PropertyImage
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid PropertyId { get; set; }
    public virtual Property Property { get; set; } = null!;
    
    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string AltText { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
