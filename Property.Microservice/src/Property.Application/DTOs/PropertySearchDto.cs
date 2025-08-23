namespace Property.Application.DTOs;

public class PropertySearchDto
{
    // Localisation (Où voulez-vous aller ?)
    public string? Location { get; set; }  // Ville, quartier, adresse
    
    // Période (Sélectionner vos dates)
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    
    // Capacité (Ajouter des voyageurs)
    public int? Guests { get; set; }  // Nombre de voyageurs/personnes
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
