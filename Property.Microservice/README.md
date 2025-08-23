# Property Microservice

Ce microservice gère les biens immobiliers dans l'écosystème Pioloop. Il fournit des fonctionnalités CRUD complètes pour la gestion des propriétés, incluant les images et équipements.

## Architecture

Le microservice suit l'architecture Clean Architecture avec les couches suivantes :

- **Domain** : Entités, interfaces de repository et logique métier
- **Application** : Cas d'usage, DTOs, handlers MediatR et mapping AutoMapper
- **Infrastructure** : Implémentation des repositories, DbContext et configuration Entity Framework
- **API** : Contrôleurs REST, configuration des services et middleware

## Fonctionnalités

### Interface de recherche simplifiée
La recherche est basée sur une interface utilisateur intuitive avec trois critères principaux :
- **📍 Où voulez-vous aller ?** : Recherche par localisation (ville, quartier, adresse)
- **📅 Sélectionner vos dates** : Filtrage par disponibilité et période
- **👥 Ajouter des voyageurs** : Filtrage par capacité d'accueil

### Entités principales
- **Property** : Bien immobilier avec toutes ses caractéristiques
- **PropertyImage** : Images associées aux biens
- **PropertyAmenity** : Équipements et commodités des biens

### Opérations CRUD
- ✅ Créer un nouveau bien
- ✅ Recherche simplifiée avec interface utilisateur intuitive
- ✅ Récupérer un bien par ID
- ✅ Mettre à jour un bien existant
- ✅ Supprimer un bien

### Types de biens supportés
- Appartement, Maison, Villa, Studio, Penthouse, Duplex, Townhouse, Terrain, Commercial, Industriel

### Statuts de biens
- Disponible, Loué, Vendu, Sous contrat, En maintenance, Hors marché

### Équipements et commodités
- **Types** : Basic, Luxury, Security, Entertainment, Utility, Outdoor, Maintenance
- **Catégories** : Plus de 50 catégories incluant les spécificités locales (Générateur, Réservoir d'eau, etc.)
- **Fonctionnalités** : Coûts additionnels, inclusion dans le loyer, priorité d'affichage, icônes

## Technologies utilisées

- **.NET 8.0** : Framework principal
- **Entity Framework Core 8.0** : ORM pour la persistance des données
- **PostgreSQL** : Base de données relationnelle
- **MediatR** : Pattern Mediator pour les commandes et requêtes
- **AutoMapper** : Mapping entre entités et DTOs
- **Swagger/OpenAPI** : Documentation de l'API
- **Docker** : Conteneurisation

## Structure du projet

```
Property.Microservice/
├── src/
│   ├── Property.API/              # Couche API
│   │   ├── Controllers/           # Contrôleurs REST
│   │   ├── Program.cs            # Configuration des services
│   │   └── appsettings.json      # Configuration
│   ├── Property.Application/       # Couche Application
│   │   ├── Commands/             # Commandes MediatR
│   │   ├── Queries/              # Requêtes MediatR
│   │   ├── Handlers/             # Gestionnaires MediatR
│   │   ├── DTOs/                 # Objets de transfert
│   │   └── Mapping/              # Profils AutoMapper
│   ├── Property.Domain/           # Couche Domain
│   │   ├── Entities/             # Entités du domaine
│   │   └── Repositories/         # Interfaces des repositories
│   └── Property.Infrastructure/   # Couche Infrastructure
│       ├── Data/                 # DbContext et configuration EF
│       └── Repositories/         # Implémentations des repositories
├── Dockerfile                     # Configuration Docker
├── docker-compose.yml            # Orchestration des services
└── Property.Microservice.sln     # Solution Visual Studio
```

## Configuration

### Variables d'environnement

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=property-db;Database=pioloop_property;Username=pioloop12345;Password=piolOOP_&!?12345;Port=5432"
  },
  "Auth": {
    "Authority": "http://auth-microservice",
    "Audience": "pioloop-api"
  }
}
```

### Ports
- **Microservice** : 5003
- **Base de données** : 5434

## API Endpoints

### Recherche de propriétés (Interface simplifiée)
- `GET /api/property` - Recherche de biens avec critères simples
  - **Paramètres de recherche :**
    - `location` : Ville, quartier ou adresse (ex: "Douala", "Akwa")
    - `checkInDate` : Date d'arrivée (ex: "2024-01-15")
    - `checkOutDate` : Date de départ (ex: "2024-01-20")
    - `guests` : Nombre de voyageurs (ex: 4)
    - `page` : Numéro de page (défaut: 1)
    - `pageSize` : Taille de page (défaut: 20)

### Gestion des propriétés
- `GET /api/property/{id}` - Récupérer un bien par ID
- `POST /api/property` - Créer un nouveau bien
- `PUT /api/property/{id}` - Mettre à jour un bien
- `DELETE /api/property/{id}` - Supprimer un bien

### Modèles de données

#### CreatePropertyDto
```json
{
  "title": "Appartement moderne à louer à Akwa",
  "description": "Magnifique appartement avec vue sur le fleuve Wouri, situé dans le quartier huppé d'Akwa. Idéal pour expatriés et professionnels",
  "price": 350000.00,
  "address": "123 Boulevard de l'Indépendance",
  "city": "Douala",
  "postalCode": "237",
  "country": "Cameroun",
  "bedrooms": 3,
  "bathrooms": 2,
  "squareMeters": 120,
  "type": "Apartment",
  "ownerId": "guid-here"
}
```

#### PropertyDto (réponse)
```json
{
  "id": "guid-here",
  "title": "Appartement moderne à louer à Akwa",
  "description": "Magnifique appartement avec vue sur le fleuve Wouri, situé dans le quartier huppé d'Akwa. Idéal pour expatriés et professionnels",
  "price": 350000.00,
  "address": "123 Boulevard de l'Indépendance",
  "city": "Douala",
  "postalCode": "237",
  "country": "Cameroun",
  "bedrooms": 3,
  "bathrooms": 2,
  "squareMeters": 120,
  "type": "Apartment",
  "status": "Available",
  "ownerId": "guid-here",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null,
  "images": [],
  "amenities": [
    {
      "id": "amenity-guid-1",
      "name": "Climatisation",
      "description": "Climatisation centrale dans toutes les pièces",
      "type": "Basic",
      "category": "AirConditioning",
      "isAvailable": true,
      "isIncludedInRent": true,
      "additionalCost": null,
      "icon": "ac-icon",
      "priority": 1,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": null
    },
    {
      "id": "amenity-guid-2",
      "name": "Générateur",
      "description": "Groupe électrogène de secours",
      "type": "Utility",
      "category": "Generator",
      "isAvailable": true,
      "isIncludedInRent": true,
      "additionalCost": null,
      "icon": "generator-icon",
      "priority": 2,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": null
    },
    {
      "id": "amenity-guid-3",
      "name": "Piscine",
      "description": "Piscine privée avec vue sur le fleuve",
      "type": "Luxury",
      "category": "SwimmingPool",
      "isAvailable": true,
      "isIncludedInRent": true,
      "additionalCost": null,
      "icon": "pool-icon",
      "priority": 3,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": null
    }
  ]
}
```

## Déploiement

### Avec Docker Compose (standalone)

```bash
cd Property.Microservice
docker-compose up -d
```

### Avec l'API Gateway complet

```bash
cd ApiGateway
docker-compose up -d
```

## Développement

### Prérequis
- .NET 8.0 SDK
- Docker et Docker Compose
- PostgreSQL (optionnel pour le développement local)

### Build et test

```bash
# Restaurer les packages
dotnet restore

# Build de la solution
dotnet build

# Exécuter les tests (si disponibles)
dotnet test

# Exécuter l'API
cd src/Property.API
dotnet run
```

### Migrations Entity Framework

```bash
cd src/Property.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Intégration avec l'API Gateway

Le microservice Property est intégré à l'API Gateway Pioloop et accessible via les routes suivantes :

- **Route principale** : `/api/property/*`
- **Swagger** : Intégré dans l'API Gateway avec la clé `property`
- **Réseau** : Connecté au réseau `pioloop-network`

## Sécurité

- **Authentification** : Intégration avec le microservice Auth via JWT
- **Autorisation** : Basée sur les claims JWT et l'audience
- **Validation** : Validation des données d'entrée avec FluentValidation

## Monitoring et santé

- **Health Check** : Endpoint `/health` pour la surveillance
- **Logs** : Logs structurés via Serilog
- **Métriques** : Métriques de performance et d'utilisation

## Exemples d'utilisation de la recherche

### Recherche simple par localisation
```bash
GET /api/property?location=Douala
```

### Recherche avec dates et voyageurs
```bash
GET /api/property?location=Akwa&checkInDate=2024-02-01&checkOutDate=2024-02-05&guests=3
```

### Recherche avec pagination
```bash
GET /api/property?location=Bonamoussadi&page=2&pageSize=10
```

### Réponse de recherche
```json
{
  "properties": [
    {
      "id": "property-guid-1",
      "title": "Appartement moderne à Akwa",
      "description": "Bel appartement avec vue sur la mer",
      "price": 150000,
      "address": "123 Boulevard de l'Indépendance",
      "city": "Douala",
      "country": "Cameroun",
      "bedrooms": 3,
      "bathrooms": 2,
      "squareMeters": 120,
      "type": "Apartment",
      "status": "Available",
      "images": [...],
      "amenities": [...]
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 20,
  "totalPages": 2,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## Support et maintenance

Pour toute question ou problème :
1. Consulter la documentation de l'API via Swagger
2. Vérifier les logs du conteneur Docker
3. Contacter l'équipe de développement Pioloop

---

**Version** : 1.0.0  
**Dernière mise à jour** : Janvier 2025  
**Maintenu par** : Équipe Pioloop
