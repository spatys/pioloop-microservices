namespace Property.Application.DTOs.Response;

public class AmenityResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
}
