namespace Property.Application.DTOs.Request;

public class PropertySearchCriteriaRequest
{
    public string? Location { get; set; } // Ex: "Bonabéri, Douala, Littoral"
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? Guests { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? PropertyType { get; set; }
    public string? RoomType { get; set; }
    public string[]? Amenities { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
