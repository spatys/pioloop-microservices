namespace Property.Application.DTOs.Request;

public class CreatePropertyRequest
{
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
    public decimal CleaningFee { get; set; } = 0;
    public decimal ServiceFee { get; set; } = 0;
    public Guid? OwnerId { get; set; } // Optionnel, sera récupéré automatiquement depuis le token JWT
    public List<PropertyAmenityRequest> Amenities { get; set; } = new();
    public List<PropertyImageRequest> Images { get; set; } = new();
}

public class PropertyAmenityRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Category { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsIncludedInRent { get; set; } = true;
    public decimal? AdditionalCost { get; set; }
    public string Icon { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
}

public class PropertyImageRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public bool IsMainImage { get; set; } = false;
    public int DisplayOrder { get; set; } = 1;
}
