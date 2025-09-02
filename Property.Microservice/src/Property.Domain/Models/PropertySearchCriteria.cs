using Property.Domain.Enums;

namespace Property.Domain.Models;

public class PropertySearchCriteria
{
    public string? Location { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? Guests { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Critères de tri
    public SortBy SortBy { get; set; } = SortBy.CreatedAt;
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;
}
