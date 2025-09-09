using System.ComponentModel.DataAnnotations;

namespace Property.Domain.Entities;

public class PropertyImage
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid PropertyId { get; set; }
    public virtual Property Property { get; set; } = null!;
    
    [Required]
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    
    [MaxLength(50)]
    public string ContentType { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string AltText { get; set; } = string.Empty;
    
    public bool IsMainImage { get; set; } = false;
    
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
