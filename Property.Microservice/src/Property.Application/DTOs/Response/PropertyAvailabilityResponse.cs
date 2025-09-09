namespace Property.Application.DTOs.Response;

public class PropertyAvailabilityResponse
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public bool IsAvailable { get; set; }
    public decimal? SpecialPrice { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PropertyAvailabilityCalendarResponse
{
    public Guid PropertyId { get; set; }
    public List<AvailabilityDay> Calendar { get; set; } = new();
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "XAF";
}

public class AvailabilityDay
{
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public decimal Price { get; set; }
    public string? Notes { get; set; }
    public bool IsToday { get; set; }
    public bool IsPast { get; set; }
    public bool IsSpecialPrice { get; set; }
}
