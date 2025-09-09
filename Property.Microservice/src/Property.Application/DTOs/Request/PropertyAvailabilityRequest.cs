namespace Property.Application.DTOs.Request;

public class CreatePropertyAvailabilityRequest
{
    public Guid PropertyId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public decimal? SpecialPrice { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePropertyAvailabilityRequest
{
    public Guid Id { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public bool IsAvailable { get; set; }
    public decimal? SpecialPrice { get; set; }
    public string? Notes { get; set; }
}

public class BulkUpdateAvailabilityRequest
{
    public Guid PropertyId { get; set; }
    public List<AvailabilityPeriod> Periods { get; set; } = new();
}

public class AvailabilityPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public decimal? SpecialPrice { get; set; }
    public string? Notes { get; set; }
}

public class GetAvailabilityRequest
{
    public Guid PropertyId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
