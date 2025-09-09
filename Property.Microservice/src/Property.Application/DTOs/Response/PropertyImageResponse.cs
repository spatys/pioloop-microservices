namespace Property.Application.DTOs.Response;

public class PropertyImageResponse
{
    public Guid Id { get; set; }
    public string ImageData { get; set; } = string.Empty; // Base64 encoded BLOB data
    public string ContentType { get; set; } = string.Empty; // MIME type
    public string AltText { get; set; } = string.Empty;
    public bool IsMainImage { get; set; }
    public int DisplayOrder { get; set; }
}
