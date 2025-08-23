using Property.Domain.Entities;

namespace Property.Application.DTOs.Response;

// Résultat de la recherche avec pagination
public class PropertySearchResponse
{
    public IEnumerable<PropertyResponse> Properties { get; set; } = Enumerable.Empty<PropertyResponse>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
