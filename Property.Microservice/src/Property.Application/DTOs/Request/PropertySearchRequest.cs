namespace Property.Application.DTOs.Request;

// Critères de recherche pour l'endpoint GET
public class PropertySearchRequest
{
    public string? Location { get; set; } // Ex: "Bonabéri, Douala, Littoral"
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public int? Guests { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
