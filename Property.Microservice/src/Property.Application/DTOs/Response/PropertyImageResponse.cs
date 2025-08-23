namespace Property.Application.DTOs.Response;

public class PropertyImageResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMainImage { get; set; }
    public int DisplayOrder { get; set; }
}
