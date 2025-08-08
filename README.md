# 🏗️ Pioloop Microservices Architecture

## 📋 Vue d'ensemble

Cette solution contient la migration de l'architecture monolithique vers une architecture microservices pour Pioloop. Chaque microservice suit les principes de Clean Architecture et Domain-Driven Design.

## 🏛️ Architecture

### Structure des microservices

```
Pioloop.Microservices/
├── Auth.Microservice/           # 🔐 Authentification, Users, Roles
│   ├── src/
│   │   ├── Auth.API/           # Controllers, Middleware
│   │   ├── Auth.Application/   # Commands, Queries, DTOs
│   │   ├── Auth.Domain/        # Entities, Services, Events
│   │   └── Auth.Infrastructure/ # Repositories, External Services
├── Property.Microservice/       # 🏠 Properties, Search, Images
│   ├── src/
│   │   ├── Property.API/
│   │   ├── Property.Application/
│   │   ├── Property.Domain/
│   │   └── Property.Infrastructure/
├── Reservation.Microservice/    # 📅 Reservations, Availability
│   ├── src/
│   │   ├── Reservation.API/
│   │   ├── Reservation.Application/
│   │   ├── Reservation.Domain/
│   │   └── Reservation.Infrastructure/
├── Payment.Microservice/        # 💳 Payments, Stripe Integration
│   ├── src/
│   │   ├── Payment.API/
│   │   ├── Payment.Application/
│   │   ├── Payment.Domain/
│   │   └── Payment.Infrastructure/
└── Email.Microservice/          # 📧 Notifications, Templates
    ├── src/
    │   ├── Email.API/
    │   ├── Email.Application/
    │   ├── Email.Domain/
    │   └── Email.Infrastructure/
```

## 🎯 Principes d'architecture

### Clean Architecture
Chaque microservice suit les principes de Clean Architecture :

- **Domain Layer** : Entités métier, services de domaine, événements
- **Application Layer** : Cas d'usage, commandes, requêtes (CQRS)
- **Infrastructure Layer** : Repositories, services externes, base de données
- **API Layer** : Controllers, middleware, configuration

### Communication inter-services
- **Synchronous** : HTTP/gRPC pour les appels directs
- **Asynchronous** : Events via Message Queue (RabbitMQ/Kafka)
- **API Gateway** : Point d'entrée unique pour tous les services

## 🚀 Démarrage rapide

### Prérequis
- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL
- Redis (optionnel pour le cache)

### Installation
```bash
# Cloner le repository
git clone <repository-url>
cd Pioloop.Microservices

# Restaurer les packages
dotnet restore

# Construire la solution
dotnet build

# Lancer les services (avec Docker Compose)
docker-compose up -d
```

## 📊 Services et responsabilités

### 🔐 Auth Microservice
**Port** : 5001
**Responsabilités** :
- Authentification JWT
- Gestion des utilisateurs
- Rôles et permissions
- Email verification

**Endpoints** :
- `POST /api/auth/login`
- `POST /api/auth/register`
- `GET /api/auth/me`
- `POST /api/auth/logout`

### 🏠 Property Microservice
**Port** : 5002
**Responsabilités** :
- Gestion des propriétés
- Recherche et filtres
- Images et médias
- Géolocalisation

**Endpoints** :
- `GET /api/properties`
- `GET /api/properties/{id}`
- `POST /api/properties`
- `PUT /api/properties/{id}`

### 📅 Reservation Microservice
**Port** : 5003
**Responsabilités** :
- Réservations
- Calendrier de disponibilité
- Gestion des conflits
- Notifications temps réel

**Endpoints** :
- `GET /api/reservations`
- `POST /api/reservations`
- `PUT /api/reservations/{id}`
- `GET /api/availability/{propertyId}`

### 💳 Payment Microservice
**Port** : 5004
**Responsabilités** :
- Intégration Stripe
- Gestion des paiements
- Refunds et disputes
- Rapports financiers

**Endpoints** :
- `POST /api/payments/process`
- `POST /api/payments/refund`
- `GET /api/payments/{id}`
- `GET /api/payments/reports`

### 📧 Email Microservice
**Port** : 5005
**Responsabilités** :
- Envoi d'emails
- Templates personnalisables
- Notifications
- Gestion des queues

**Endpoints** :
- `POST /api/email/send`
- `POST /api/email/templates`
- `GET /api/email/status/{id}`

## 🔧 Configuration

### Variables d'environnement
Chaque microservice utilise des variables d'environnement pour la configuration :

```env
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=pioloop_auth;Username=postgres;Password=password

# JWT
Jwt__Secret=your-secret-key
Jwt__Issuer=pioloop
Jwt__Audience=pioloop

# External Services
Stripe__SecretKey=sk_test_...
Email__SmtpServer=smtp.gmail.com
Email__SmtpPort=587
```

### Docker Compose
```yaml
version: '3.8'
services:
  auth-service:
    build: ./Auth.Microservice
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  
  property-service:
    build: ./Property.Microservice
    ports:
      - "5002:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  
  # ... autres services
```

## 🧪 Tests

### Tests unitaires
```bash
# Lancer tous les tests
dotnet test

# Tests spécifiques
dotnet test Auth.Microservice/tests/Auth.UnitTests/
```

### Tests d'intégration
```bash
# Tests d'intégration
dotnet test Auth.Microservice/tests/Auth.IntegrationTests/
```

## 📈 Monitoring

### Health Checks
Chaque service expose des health checks :
- `GET /health` : Statut général
- `GET /health/ready` : Prêt à recevoir du trafic
- `GET /health/live` : Service en vie

### Logging
- **Structured Logging** avec Serilog
- **Centralized Logging** avec ELK Stack
- **Distributed Tracing** avec Jaeger

### Metrics
- **Prometheus** pour les métriques
- **Grafana** pour la visualisation

## 🔄 Migration depuis l'architecture monolithique

### Phase 1 : Extraction des services
1. ✅ Auth Microservice (en cours)
2. 🔄 Property Microservice
3. ⏳ Reservation Microservice
4. ⏳ Payment Microservice
5. ⏳ Email Microservice

### Phase 2 : Communication inter-services
1. API Gateway
2. Message Queue
3. Event Sourcing

### Phase 3 : Optimisation
1. Caching
2. Circuit Breakers
3. Performance tuning

## 📚 Documentation

- [Architecture Microservices](./MICROSERVICES_ARCHITECTURE.md)
- [API Documentation](./docs/api/)
- [Deployment Guide](./docs/deployment/)

## 🤝 Contribution

1. Fork le repository
2. Créer une branche feature (`git checkout -b feature/amazing-feature`)
3. Commit les changements (`git commit -m 'Add amazing feature'`)
4. Push vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrir une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

---

**Note** : Cette architecture est en cours de développement. Certains services peuvent ne pas être encore complètement implémentés. 