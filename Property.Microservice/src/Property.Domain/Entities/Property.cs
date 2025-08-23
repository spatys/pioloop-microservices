using System.ComponentModel.DataAnnotations;

namespace Property.Domain.Entities;

public class Property
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int SquareMeters { get; set; }
    
    [Required]
    public PropertyType Type { get; set; }
    
    [Required]
    public PropertyStatus Status { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public virtual ICollection<PropertyAmenity> Amenities { get; set; } = new List<PropertyAmenity>();
}

public enum PropertyType
{
    Apartment,
    House,
    Villa,
    Studio,
    Duplex
}

public enum PropertyStatus
{
    Available,
    Rented,
    Maintenance
}
