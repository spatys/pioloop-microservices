3# 🔐 Auth Microservice

## 📋 Vue d'ensemble

Le microservice Auth est responsable de la gestion de l'authentification et de l'autorisation dans l'architecture Pioloop. Il fournit des fonctionnalités complètes pour l'inscription, la connexion, la vérification d'email et la gestion des utilisateurs.

## 🏗️ Architecture

### Clean Architecture
Le microservice suit les principes de Clean Architecture avec les couches suivantes :

- **Domain Layer** : Entités métier, interfaces de repository, services de domaine
- **Application Layer** : Cas d'usage, commandes/requêtes CQRS, DTOs
- **Infrastructure Layer** : Implémentation des repositories, DbContext, services externes
- **API Layer** : Controllers, configuration, middleware

### Structure du projet
```
Auth.Microservice/
├── src/
│   ├── Auth.API/              # Controllers, Program.cs, configuration
│   ├── Auth.Application/      # Commands, Queries, DTOs, Handlers
│   ├── Auth.Domain/           # Entities, Interfaces, Services
│   └── Auth.Infrastructure/   # Repositories, DbContext, Services
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## 🚀 Démarrage rapide

### Prérequis
- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL (optionnel si vous utilisez Docker)

### Installation avec Docker
```bash
# Cloner le repository
cd Auth.Microservice

# Lancer les services
docker-compose up -d

# Vérifier que les services sont en cours d'exécution
docker-compose ps
```

### Installation locale
```bash
# Restaurer les packages
dotnet restore

# Construire la solution
dotnet build

# Configurer la base de données
# Assurez-vous que PostgreSQL est en cours d'exécution
# Modifiez la chaîne de connexion dans appsettings.json

# Lancer l'API
dotnet run --project src/Auth.API
```

## 📊 Endpoints API

### Authentification
- `POST /api/auth/login` - Connexion utilisateur
- `POST /api/auth/register` - Inscription utilisateur
- `POST /api/auth/verify-email` - Vérification d'email

### Gestion des utilisateurs
- `GET /api/auth/users/{id}` - Récupérer un utilisateur par ID
- `GET /api/auth/users/email/{email}` - Récupérer un utilisateur par email

### Santé du service
- `GET /api/auth/health` - Vérifier l'état du service

## 🔧 Configuration

### Variables d'environnement
```env
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=pioloop_auth;Username=postgres;Password=password;Port=5432

# JWT
Jwt__SecretKey=Pioloop2025SecretKey32CharsLong!@#
Jwt__Issuer=Pioloop
Jwt__Audience=PioloopUsers
Jwt__ExpirationHours=24
```

### Configuration Docker
Le fichier `docker-compose.yml` configure automatiquement :
- API sur le port 5001
- PostgreSQL sur le port 5433
- Variables d'environnement pour le développement

## 🗄️ Base de données

### Entités principales
- **User** : Informations utilisateur, vérification email, consentement
- **Role** : Rôles système (Admin, User, Owner)
- **UserRole** : Relation many-to-many entre User et Role
- **UserPassword** : Mots de passe hashés avec salt

### Migration
```bash
# Créer une migration
dotnet ef migrations add InitialCreate --project src/Auth.Infrastructure --startup-project src/Auth.API

# Appliquer les migrations
dotnet ef database update --project src/Auth.Infrastructure --startup-project src/Auth.API
```

## 🔐 Sécurité

### JWT Authentication
- Tokens signés avec HMAC-SHA256
- Validation complète (issuer, audience, expiration)
- Claims personnalisés pour les informations utilisateur

### Password Hashing
- Hachage SHA256 avec salt aléatoire
- Salt unique par utilisateur
- Vérification sécurisée des mots de passe

### Email Verification
- Codes de vérification à 6 chiffres
- Expiration automatique (10 minutes par défaut)
- Protection contre les tentatives multiples (blocage après 5 échecs)

## 🧪 Tests

### Tests unitaires
```bash
# Lancer les tests
dotnet test

# Tests avec couverture
dotnet test --collect:"XPlat Code Coverage"
```

### Tests d'intégration
```bash
# Tests d'intégration avec base de données
dotnet test --filter Category=Integration
```

## 📈 Monitoring

### Health Checks
- Endpoint `/api/auth/health` pour vérifier l'état du service
- Vérification de la connectivité à la base de données
- Métriques de performance

### Logging
- Logging structuré avec Serilog
- Niveaux de log configurables
- Intégration avec ELK Stack

## 🔄 Communication inter-services

### Événements de domaine
- `UserRegisteredEvent` : Utilisateur inscrit
- `UserEmailVerifiedEvent` : Email vérifié
- `UserLoginEvent` : Connexion utilisateur

### API Gateway
Le microservice est conçu pour être consommé via un API Gateway qui :
- Gère l'authentification JWT
- Route les requêtes vers les services appropriés
- Fournit un point d'entrée unique

## 🚀 Déploiement

### Production
```bash
# Build pour la production
docker build -t pioloop/auth-service:latest .

# Déployer avec docker-compose
docker-compose -f docker-compose.prod.yml up -d
```

### Variables d'environnement de production
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=prod-db;Database=pioloop_auth;Username=prod_user;Password=secure_password
Jwt__SecretKey=your-secure-production-secret-key
```

## 🤝 Contribution

1. Fork le repository
2. Créer une branche feature (`git checkout -b feature/amazing-feature`)
3. Commit les changements (`git commit -m 'Add amazing feature'`)
4. Push vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrir une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](../../LICENSE) pour plus de détails.

---

**Note** : Ce microservice fait partie de l'architecture Pioloop. Pour plus d'informations sur l'architecture globale, consultez le [README principal](../../README.md).
