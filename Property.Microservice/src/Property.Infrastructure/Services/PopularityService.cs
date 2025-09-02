using Property.Domain.Interfaces;
using Property.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Property.Infrastructure.Services;

public class PopularityService : IPopularityService
{
    private readonly PropertyDbContext _context;

    public PopularityService(PropertyDbContext context)
    {
        _context = context;
    }

    public double CalculatePopularityScore(int viewCount, int reservationCount, double averageRating, int totalReviews)
    {
        // Facteurs de pondération pour chaque métrique
        const double viewWeight = 0.3;      // 30% - Vues
        const double reservationWeight = 0.4; // 40% - Réservations  
        const double ratingWeight = 0.25;    // 25% - Note moyenne
        const double reviewWeight = 0.05;    // 5% - Nombre d'avis

        // Normalisation des métriques
        var normalizedViews = Math.Min(viewCount / 100.0, 1.0); // Max 100 vues = 100%
        var normalizedReservations = Math.Min(reservationCount / 20.0, 1.0); // Max 20 réservations = 100%
        var normalizedRating = averageRating / 5.0; // Note sur 5
        var normalizedReviews = Math.Min(totalReviews / 50.0, 1.0); // Max 50 avis = 100%

        // Calcul du score pondéré
        var score = (normalizedViews * viewWeight) +
                   (normalizedReservations * reservationWeight) +
                   (normalizedRating * ratingWeight) +
                   (normalizedReviews * reviewWeight);

        // Retourner un score entre 0 et 100
        return Math.Round(score * 100, 2);
    }

    public async Task<bool> UpdatePopularityScoreAsync(Guid propertyId)
    {
        try
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                return false;

            // Calculer le nouveau score de popularité
            var newScore = CalculatePopularityScore(
                property.ViewCount,
                property.ReservationCount,
                property.AverageRating,
                property.TotalReviews
            );

            // Mettre à jour le score
            property.PopularityScore = newScore;
            property.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
