namespace Property.Application.DTOs.Response;

public class PropertyAmenityResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsIncludedInRent { get; set; }
    public decimal? AdditionalCost { get; set; }
    public string? Icon { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
