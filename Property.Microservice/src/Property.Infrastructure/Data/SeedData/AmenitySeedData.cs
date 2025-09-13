using Microsoft.EntityFrameworkCore;
using Property.Domain.Entities;
using Property.Infrastructure.Data;

namespace Property.Infrastructure.Data.SeedData;

public static class AmenitySeedData
{
    public static async Task SeedAmenitiesAsync(PropertyDbContext context)
    {
        // Supprimer les amenities existantes pour les remplacer par celles d'Airbnb
        var existingAmenities = await context.Amenities.ToListAsync();
        if (existingAmenities.Any())
        {
            Console.WriteLine("Removing existing amenities to replace with Airbnb-style amenities...");
            context.Amenities.RemoveRange(existingAmenities);
            await context.SaveChangesAsync();
        }

        var amenities = new List<Amenity>
        {
            // Essentiels (Basics)
            new Amenity { Name = "Wi-Fi", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Cuisine équipée", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Lave-linge", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Sèche-linge", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Chauffage", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Climatisation", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Lave-vaisselle", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Réfrigérateur", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Micro-ondes", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Cafetière", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Ustensiles de cuisine", Category = "Essentiels", IsActive = true },
            new Amenity { Name = "Vaisselle", Category = "Essentiels", IsActive = true },

            // Sécurité (Safety)
            new Amenity { Name = "Détecteur de fumée", Category = "Sécurité", IsActive = true },
            new Amenity { Name = "Trousse de premiers secours", Category = "Sécurité", IsActive = true },
            new Amenity { Name = "Extincteur", Category = "Sécurité", IsActive = true },
            new Amenity { Name = "Serrure de porte", Category = "Sécurité", IsActive = true },
            new Amenity { Name = "Arrivée autonome", Category = "Sécurité", IsActive = true },

            // Extérieur (Outdoor)
            new Amenity { Name = "Parking gratuit", Category = "Extérieur", IsActive = true },
            new Amenity { Name = "Jardin", Category = "Extérieur", IsActive = true },
            new Amenity { Name = "Piscine", Category = "Extérieur", IsActive = true },
            new Amenity { Name = "Terrasse", Category = "Extérieur", IsActive = true },
            new Amenity { Name = "Balcon", Category = "Extérieur", IsActive = true },
            new Amenity { Name = "Garage", Category = "Extérieur", IsActive = true },
            new Amenity { Name = "Cour", Category = "Extérieur", IsActive = true },

            // Famille (Family)
            new Amenity { Name = "Lit parapluie", Category = "Famille", IsActive = true },
            new Amenity { Name = "Chaise haute", Category = "Famille", IsActive = true },
            new Amenity { Name = "Table à langer", Category = "Famille", IsActive = true },
            new Amenity { Name = "Jeux pour enfants", Category = "Famille", IsActive = true },
            new Amenity { Name = "Livres pour enfants", Category = "Famille", IsActive = true },
            new Amenity { Name = "Poussette", Category = "Famille", IsActive = true },
            new Amenity { Name = "Sécurité enfants", Category = "Famille", IsActive = true },
            new Amenity { Name = "Barrières de sécurité", Category = "Famille", IsActive = true },
            new Amenity { Name = "Protège-prises", Category = "Famille", IsActive = true },

            // Accessibilité (Accessibility)
            new Amenity { Name = "Accès fauteuil roulant", Category = "Accessibilité", IsActive = true },
            new Amenity { Name = "Ascenseur", Category = "Accessibilité", IsActive = true },
            new Amenity { Name = "Rampes d'accès", Category = "Accessibilité", IsActive = true },
            new Amenity { Name = "Salle de bain adaptée", Category = "Accessibilité", IsActive = true },
            new Amenity { Name = "Barres d'appui", Category = "Accessibilité", IsActive = true },
            new Amenity { Name = "Douche à l'italienne", Category = "Accessibilité", IsActive = true },

            // Divertissement (Entertainment)
            new Amenity { Name = "Télévision", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Câble/Satellite", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Netflix", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Amazon Prime", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Disney+", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "HBO Max", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Jeux de société", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Piano", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Guitare", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Console de jeux", Category = "Divertissement", IsActive = true },
            new Amenity { Name = "Table de billard", Category = "Divertissement", IsActive = true },

            // Confort (Comfort)
            new Amenity { Name = "Équipement de fitness", Category = "Confort", IsActive = true },
            new Amenity { Name = "Vue sur mer", Category = "Confort", IsActive = true },
            new Amenity { Name = "Vue sur montagne", Category = "Confort", IsActive = true },
            new Amenity { Name = "Vue sur ville", Category = "Confort", IsActive = true },
            new Amenity { Name = "Vue sur jardin", Category = "Confort", IsActive = true },
            new Amenity { Name = "Lave-linge et sèche-linge", Category = "Confort", IsActive = true },

            // Espace de travail (Work Space)
            new Amenity { Name = "Espace de travail dédié", Category = "Espace de travail", IsActive = true },
            new Amenity { Name = "Bureau", Category = "Espace de travail", IsActive = true },
            new Amenity { Name = "Chaise de bureau", Category = "Espace de travail", IsActive = true },
            new Amenity { Name = "Éclairage de bureau", Category = "Espace de travail", IsActive = true },
            new Amenity { Name = "Wi-Fi rapide", Category = "Espace de travail", IsActive = true },
        };

        await context.Amenities.AddRangeAsync(amenities);
        await context.SaveChangesAsync();
    }
}
