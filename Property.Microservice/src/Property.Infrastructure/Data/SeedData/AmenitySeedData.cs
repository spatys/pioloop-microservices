using Microsoft.EntityFrameworkCore;
using Property.Domain.Entities;
using Property.Infrastructure.Data;

namespace Property.Infrastructure.Data.SeedData;

public static class AmenitySeedData
{
    public static async Task SeedAmenitiesAsync(PropertyDbContext context)
    {
        // Vérifier si les données existent déjà
        if (await context.Amenities.AnyAsync())
        {
            Console.WriteLine("Amenities already seeded, skipping...");
            return;
        }

        var amenities = new List<Amenity>
        {
            // Essentiels
            new Amenity { Name = "Wi-Fi", Category = "Essentiels", Icon = "📶", IsActive = true },
            new Amenity { Name = "Cuisine équipée", Category = "Essentiels", Icon = "🍳", IsActive = true },
            new Amenity { Name = "Lave-linge", Category = "Essentiels", Icon = "🧺", IsActive = true },
            new Amenity { Name = "Chauffage", Category = "Essentiels", Icon = "🔥", IsActive = true },
            new Amenity { Name = "Climatisation", Category = "Essentiels", Icon = "❄️", IsActive = true },
            new Amenity { Name = "Eau chaude", Category = "Essentiels", Icon = "🚿", IsActive = true },
            new Amenity { Name = "Électricité", Category = "Essentiels", Icon = "💡", IsActive = true },
            new Amenity { Name = "Lave-vaisselle", Category = "Essentiels", Icon = "🍽️", IsActive = true },
            new Amenity { Name = "Réfrigérateur", Category = "Essentiels", Icon = "🧊", IsActive = true },
            new Amenity { Name = "Micro-ondes", Category = "Essentiels", Icon = "⚡", IsActive = true },

            // Sécurité
            new Amenity { Name = "Détecteur de fumée", Category = "Sécurité", Icon = "🚨", IsActive = true },
            new Amenity { Name = "Détecteur de monoxyde de carbone", Category = "Sécurité", Icon = "⚠️", IsActive = true },
            new Amenity { Name = "Sécurité 24h/24", Category = "Sécurité", Icon = "🔒", IsActive = true },
            new Amenity { Name = "Caméras de surveillance", Category = "Sécurité", Icon = "📹", IsActive = true },
            new Amenity { Name = "Système d'alarme", Category = "Sécurité", Icon = "🚨", IsActive = true },
            new Amenity { Name = "Coffre-fort", Category = "Sécurité", Icon = "🔐", IsActive = true },
            new Amenity { Name = "Interphone", Category = "Sécurité", Icon = "📞", IsActive = true },

            // Extérieur
            new Amenity { Name = "Parking gratuit", Category = "Extérieur", Icon = "🅿️", IsActive = true },
            new Amenity { Name = "Jardin", Category = "Extérieur", Icon = "🌳", IsActive = true },
            new Amenity { Name = "Piscine", Category = "Extérieur", Icon = "🏊", IsActive = true },
            new Amenity { Name = "Terrasse", Category = "Extérieur", Icon = "🪑", IsActive = true },
            new Amenity { Name = "Barbecue", Category = "Extérieur", Icon = "🔥", IsActive = true },
            new Amenity { Name = "Balcon", Category = "Extérieur", Icon = "🏠", IsActive = true },
            new Amenity { Name = "Cour", Category = "Extérieur", Icon = "🏡", IsActive = true },
            new Amenity { Name = "Garage", Category = "Extérieur", Icon = "🚗", IsActive = true },
            new Amenity { Name = "Place de parking payante", Category = "Extérieur", Icon = "💰", IsActive = true },

            // Famille
            new Amenity { Name = "Équipements bébé", Category = "Famille", Icon = "👶", IsActive = true },
            new Amenity { Name = "Jeux pour enfants", Category = "Famille", Icon = "🧸", IsActive = true },
            new Amenity { Name = "Chaise haute", Category = "Famille", Icon = "🪑", IsActive = true },
            new Amenity { Name = "Lit bébé", Category = "Famille", Icon = "🛏️", IsActive = true },
            new Amenity { Name = "Table à langer", Category = "Famille", Icon = "👶", IsActive = true },
            new Amenity { Name = "Poussette", Category = "Famille", Icon = "👶", IsActive = true },
            new Amenity { Name = "Sécurité enfants", Category = "Famille", Icon = "🔒", IsActive = true },

            // Accessibilité
            new Amenity { Name = "Accès fauteuil roulant", Category = "Accessibilité", Icon = "♿", IsActive = true },
            new Amenity { Name = "Ascenseur", Category = "Accessibilité", Icon = "🛗", IsActive = true },
            new Amenity { Name = "Rampes d'accès", Category = "Accessibilité", Icon = "🛤️", IsActive = true },
            new Amenity { Name = "Porte large", Category = "Accessibilité", Icon = "🚪", IsActive = true },
            new Amenity { Name = "Salle de bain adaptée", Category = "Accessibilité", Icon = "🚿", IsActive = true },
            new Amenity { Name = "Barres d'appui", Category = "Accessibilité", Icon = "🤝", IsActive = true },

            // Divertissement
            new Amenity { Name = "Télévision", Category = "Divertissement", Icon = "📺", IsActive = true },
            new Amenity { Name = "Câble/Satellite", Category = "Divertissement", Icon = "📡", IsActive = true },
            new Amenity { Name = "Netflix", Category = "Divertissement", Icon = "🎬", IsActive = true },
            new Amenity { Name = "Jeux de société", Category = "Divertissement", Icon = "🎲", IsActive = true },
            new Amenity { Name = "Livres", Category = "Divertissement", Icon = "📚", IsActive = true },
            new Amenity { Name = "Piano", Category = "Divertissement", Icon = "🎹", IsActive = true },
            new Amenity { Name = "Guitare", Category = "Divertissement", Icon = "🎸", IsActive = true },

            // Confort
            new Amenity { Name = "Cheminée", Category = "Confort", Icon = "🔥", IsActive = true },
            new Amenity { Name = "Jacuzzi", Category = "Confort", Icon = "🛁", IsActive = true },
            new Amenity { Name = "Sauna", Category = "Confort", Icon = "🧖", IsActive = true },
            new Amenity { Name = "Salle de sport", Category = "Confort", Icon = "💪", IsActive = true },
            new Amenity { Name = "Spa", Category = "Confort", Icon = "🧘", IsActive = true },
            new Amenity { Name = "Vue sur mer", Category = "Confort", Icon = "🌊", IsActive = true },
            new Amenity { Name = "Vue sur montagne", Category = "Confort", Icon = "⛰️", IsActive = true },
            new Amenity { Name = "Vue sur ville", Category = "Confort", Icon = "🏙️", IsActive = true }
        };

        await context.Amenities.AddRangeAsync(amenities);
        await context.SaveChangesAsync();
    }
}
