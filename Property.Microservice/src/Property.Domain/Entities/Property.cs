using System.ComponentModel.DataAnnotations;

namespace Property.Domain.Entities;

public class Property
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string PropertyType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string RoomType { get; set; } = string.Empty;
    
    public int MaxGuests { get; set; }
    public int Bedrooms { get; set; }
    public int Beds { get; set; }
    public int Bathrooms { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    [Required]
    public decimal PricePerNight { get; set; }
    
    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }
    
    public bool IsInstantBookable { get; set; }
    
    [Required]
    public PropertyStatus Status { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public virtual ICollection<PropertyAmenity> Amenities { get; set; } = new List<PropertyAmenity>();
    public virtual ICollection<PropertyAvailability> Availability { get; set; } = new List<PropertyAvailability>();
}

public enum PropertyStatus
{
    Draft,
    Published,
    Rented,
    Maintenance,
    Deleted
}
