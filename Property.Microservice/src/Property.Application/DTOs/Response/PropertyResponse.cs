namespace Property.Application.DTOs.Response;

public class PropertyResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
    public int Bedrooms { get; set; }
    public int Beds { get; set; }
    public int Bathrooms { get; set; }
    public int SquareMeters { get; set; }
            public string Address { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<string> Amenities { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
