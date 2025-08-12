# 🏗️ Pioloop Microservices Ecosystem

Écosystème complet de microservices pour la plateforme Pioloop, incluant authentification, gestion d'emails et API Gateway unifié.

## 🏛️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   API Gateway   │    │  Microservices  │
│   (React/Next)  │───▶│   (Port 5000)   │───▶│                 │
│                 │    │   Ocelot + JWT  │    │ Auth (5001)     │
│                 │    │   Swagger UI    │    │ Email (5002)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🚀 Services

### 🔐 Auth Microservice (Port 5001)
- **Authentification JWT** - Login, register, vérification email
- **Gestion des utilisateurs** - CRUD utilisateurs, rôles
- **Base de données** - PostgreSQL avec Entity Framework
- **Sécurité** - Hachage SHA256, validation des tokens

### 📧 Email Microservice (Port 5002)
- **Envoi d'emails** - Templates HTML professionnels
- **Types d'emails** - Vérification, bienvenue, réinitialisation, etc.
- **SMTP** - Configuration Gmail/SendGrid
- **Validation** - Email validation, gestion d'erreurs

### 🌐 API Gateway (Port 5000)
- **Routage intelligent** - Ocelot pour router vers les microservices
- **Authentification centralisée** - JWT Bearer validation
- **Swagger unifié** - Documentation agrégée de tous les services
- **Logging complet** - Serilog avec logs structurés
- **Gestion d'erreurs** - Réponses standardisées

## 📋 Prérequis

- **Docker** - [Télécharger](https://www.docker.com/products/docker-desktop)
- **Docker Compose** - Inclus avec Docker Desktop
- **Git** - Pour cloner le repository

## 🛠️ Installation Rapide

### 1. Cloner le repository
```bash
git clone <repository-url>
cd pioloop-microservices
```

### 2. Démarrer l'écosystème complet
```bash
# Démarrer tous les services
./start-all.sh

# Arrêter tous les services
./stop-all.sh
```

**Avantages de cette approche :**
- ✅ Environnement isolé et reproductible
- ✅ Configuration centralisée
- ✅ Base de données PostgreSQL incluse
- ✅ Health checks automatiques
- ✅ Logs centralisés
- ✅ Déploiement simplifié

### 3. Accéder aux services
- **API Gateway**: http://localhost:5000
- **Swagger Unifié**: http://localhost:5000/swagger
- **Auth Service**: http://localhost:5001
- **Email Service**: http://localhost:5002
- **PostgreSQL DB**: localhost:5433

## 📡 API Endpoints

### 🔐 Authentification (via API Gateway)
```bash
# Login utilisateur
POST http://localhost:5000/api/auth/login
{
  "email": "user@example.com",
  "password": "password"
}

# Inscription utilisateur (3 étapes)
# Étape 1: Enregistrement email
POST http://localhost:5000/api/auth/register-email
{
  "email": "user@example.com"
}

# Étape 2: Vérification email
POST http://localhost:5000/api/auth/register-verify-email
{
  "email": "user@example.com",
  "code": "123456"
}

# Étape 3: Finalisation inscription
POST http://localhost:5000/api/auth/register-complete
{
  "email": "user@example.com",
  "password": "password",
  "firstName": "John",
  "lastName": "Doe"
}

# Vérification email (authentification requise)
POST http://localhost:5000/api/auth/verify-email
Authorization: Bearer <jwt-token>
{
  "email": "user@example.com",
  "code": "123456"
}
```

### 📧 Email Service (via API Gateway)
```bash
# Envoi email de vérification
POST http://localhost:5000/api/email/send-email-verification
{
  "email": "user@example.com",
  "code": "123456"
}

# Envoi email de bienvenue
POST http://localhost:5000/api/email/send-email-account-created
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe"
}
```

### 🏥 Health Checks
```bash
# API Gateway
GET http://localhost:5000/health

# Auth Service
GET http://localhost:5001/api/auth/health

# Email Service
GET http://localhost:5002/api/email/health
```

## 🔧 Configuration

### Variables d'environnement
```bash
# JWT Configuration (doit être identique dans tous les services)
JwtSettings__SecretKey=Pioloop2025SecretKey32CharsLong!@#
JwtSettings__Issuer=Pioloop
JwtSettings__Audience=PioloopUsers
JwtSettings__ExpirationHours=24

# Database (Auth Service)
ConnectionStrings__DefaultConnection=Host=localhost;Database=pioloop_auth;Username=postgres;Password=password;Port=5433

# SMTP (Email Service)
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=your-email@gmail.com
EmailSettings__SmtpPassword=your-app-password
```

### Configuration Ocelot
Le fichier `ApiGateway/ocelot.json` configure :
- Routes vers les microservices
- Authentification JWT
- Swagger aggregation
- Endpoints publics vs protégés

## 🚀 Démarrage Manuel

### Script automatique (Recommandé)
```bash
./start-all.sh
```

### Démarrage manuel avec Docker Compose
```bash
# Démarrer l'écosystème complet
cd ApiGateway
docker-compose up -d

# Voir les logs
docker-compose logs -f

# Arrêter
docker-compose down
```

## 🛑 Arrêt des Services

### Arrêt automatique
```bash
./stop-all.sh
```

### Arrêt manuel
```bash
cd ApiGateway
docker-compose down
```

## 📊 Monitoring et Logs

### Logs
- **Console** - Logs en temps réel
- **Fichiers** - `logs/service-name.log`
- **Niveaux** - Information, Warning, Error

### Health Checks
```bash
# Vérifier l'état de tous les services
curl http://localhost:5000/health
curl http://localhost:5001/api/auth/health
curl http://localhost:5002/api/email/health
```

### Métriques
- Temps de réponse des requêtes
- Taux d'erreur par service
- Utilisation des ressources

## 🔐 Sécurité

### Authentification JWT
- **Signature** - HMAC-SHA256
- **Expiration** - 24h par défaut
- **Claims** - User ID, email, rôles
- **Validation** - Issuer, audience, expiration

### CORS
- **API Gateway** - Configuration permissive pour le développement
- **Microservices** - Restreint aux API Gateways uniquement

### Validation
- **Input validation** - Modèles de données validés
- **Email validation** - Format et domaine vérifiés
- **Password strength** - Règles de complexité

## 🧪 Tests

### Tests d'intégration
```bash
# Test de santé
curl http://localhost:5000/health

# Test d'authentification
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'

# Test d'envoi d'email
curl -X POST "http://localhost:5000/api/email/send-email-verification" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","code":"123456"}'
```

### Tests Swagger
- Accéder à http://localhost:5000/swagger
- Tester les endpoints directement depuis l'interface
- Authentification JWT intégrée

## 🔧 Développement

### Structure des projets
```
pioloop-microservices/
├── Auth.Microservice/          # Service d'authentification
│   ├── src/
│   │   ├── Auth.API/          # Controllers, Program.cs
│   │   ├── Auth.Application/  # Commands, Queries, DTOs
│   │   ├── Auth.Domain/       # Entities, Interfaces
│   │   └── Auth.Infrastructure/ # Repositories, DbContext
│   └── docker-compose.yml
├── Email.Microservice/         # Service d'emails
│   ├── src/
│   │   ├── Email.API/         # Controllers, Program.cs
│   │   ├── Email.Application/ # Handlers, DTOs
│   │   ├── Email.Domain/      # Interfaces
│   │   └── Email.Infrastructure/ # Services
│   └── docker-compose.yml
├── ApiGateway/                 # API Gateway unifié
│   ├── Controllers/           # Health, Info
│   ├── Middleware/            # Error handling, Logging
│   ├── Models/                # ApiResponse
│   ├── ocelot.json           # Configuration Ocelot
│   └── Program.cs            # Configuration complète
├── start-all.sh              # Script de démarrage
├── stop-all.sh               # Script d'arrêt
└── README.md                 # Ce fichier
```

### Ajout d'un nouveau microservice
1. Créer le projet dans un nouveau dossier
2. Ajouter la route dans `ApiGateway/ocelot.json`
3. Configurer Swagger aggregation
4. Mettre à jour les scripts de démarrage

## 🚀 Déploiement

### Production
```bash
# Build pour la production
dotnet publish -c Release

# Variables d'environnement de production
export ASPNETCORE_ENVIRONMENT=Production
export JwtSettings__SecretKey=your-secure-production-key

# Démarrage
dotnet ./publish/ApiGateway/ApiGateway.dll
dotnet ./publish/Auth.Microservice/src/Auth.API/Auth.API.dll
dotnet ./publish/Email.Microservice/src/Email.API/Email.API.dll
```

### Docker
```bash
# Build des images
docker build -t pioloop/auth-service ./Auth.Microservice
docker build -t pioloop/email-service ./Email.Microservice
docker build -t pioloop/api-gateway ./ApiGateway

# Démarrage avec docker-compose
docker-compose up -d
```

## 🤝 Contribution

1. Fork le repository
2. Créez une branche feature (`git checkout -b feature/amazing-feature`)
3. Committez vos changements (`git commit -m 'Add amazing feature'`)
4. Push vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrez une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

## 📞 Support

- **Email**: support@pioloop.com
- **Documentation**: https://docs.pioloop.com
- **Issues**: [GitHub Issues](https://github.com/your-repo/issues)

---

**Note** : Cet écosystème est conçu pour être évolutif et maintenable. Chaque microservice peut être développé et déployé indépendamment.
