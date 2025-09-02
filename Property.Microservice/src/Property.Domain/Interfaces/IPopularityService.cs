namespace Property.Domain.Interfaces;

public interface IPopularityService
{
    /// <summary>
    /// Calcule le score de popularité d'une propriété basé sur ses métriques
    /// </summary>
    /// <param name="viewCount">Nombre de vues</param>
    /// <param name="reservationCount">Nombre de réservations</param>
    /// <param name="averageRating">Note moyenne</param>
    /// <param name="totalReviews">Nombre total d'avis</param>
    /// <returns>Score de popularité normalisé entre 0 et 100</returns>
    double CalculatePopularityScore(int viewCount, int reservationCount, double averageRating, int totalReviews);
    
    /// <summary>
    /// Met à jour le score de popularité d'une propriété
    /// </summary>
    /// <param name="propertyId">ID de la propriété</param>
    /// <returns>True si la mise à jour a réussi</returns>
    Task<bool> UpdatePopularityScoreAsync(Guid propertyId);
}
