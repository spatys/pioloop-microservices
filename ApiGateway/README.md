# 🌐 Pioloop API Gateway

API Gateway unifié pour l'écosystème Pioloop, basé sur Ocelot avec authentification JWT et Swagger agrégé.

## 🚀 Fonctionnalités

- **🔐 Authentification JWT** - Validation automatique des tokens Bearer
- **📚 Swagger Unifié** - Documentation agrégée de tous les microservices
- **📊 Logging Complet** - Logs structurés des requêtes et réponses
- **🛡️ Gestion d'Erreurs** - Réponses d'erreur standardisées
- **⚡ Routage Intelligent** - Routage automatique vers les microservices
- **🔒 Sécurité** - CORS, validation des tokens, protection des endpoints

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   API Gateway   │    │  Microservices  │
│                 │───▶│   (Port 5000)   │───▶│                 │
│                 │    │                 │    │ Auth (5001)     │
│                 │    │                 │    │ Email (5002)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 📋 Prérequis

- .NET 8.0 SDK
- Microservices Auth et Email en cours d'exécution
- Docker (optionnel)

## 🛠️ Installation

### Développement local

```bash
cd ApiGateway
dotnet restore
dotnet build
dotnet run
```

### Avec Docker

```bash
docker build -t pioloop-api-gateway .
docker run -p 5000:5000 pioloop-api-gateway
```

## 🔧 Configuration

### Variables d'environnement

```bash
# JWT Configuration
JwtSettings__SecretKey=Pioloop2025SecretKey32CharsLong!@#
JwtSettings__Issuer=Pioloop
JwtSettings__Audience=PioloopUsers
JwtSettings__ExpirationHours=24

# Microservices URLs
AuthService__Url=http://localhost:5001
EmailService__Url=http://localhost:5002
```

### appsettings.json

```json
{
  "JwtSettings": {
    "SecretKey": "Pioloop2025SecretKey32CharsLong!@#",
    "Issuer": "Pioloop",
    "Audience": "PioloopUsers",
    "ExpirationHours": 24
  }
}
```

## 📡 Routes API

### Authentification (Auth Service)

| Méthode | Endpoint | Description | Auth Requise |
|---------|----------|-------------|--------------|
| `POST` | `/api/auth/login` | Connexion utilisateur | ❌ |
| `POST` | `/api/auth/register-email` | Étape 1: Enregistrement email | ❌ |
| `POST` | `/api/auth/register-verify-email` | Étape 2: Vérification email | ❌ |
| `POST` | `/api/auth/register-complete` | Étape 3: Finalisation inscription | ❌ |
| `POST` | `/api/auth/verify-email` | Vérification email | ✅ |
| `GET` | `/api/auth/users/{id}` | Récupérer utilisateur | ✅ |

### Email Service

| Méthode | Endpoint | Description | Auth Requise |
|---------|----------|-------------|--------------|
| `POST` | `/api/email/send-email-verification` | Envoi email vérification | ❌ |
| `POST` | `/api/email/send-email-account-created` | Email bienvenue | ❌ |
| `POST` | `/api/email/send-email-password-reset` | Email réinitialisation | ❌ |

### API Gateway

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/health` | Health check |
| `GET` | `/api/health` | Health check détaillé |
| `GET` | `/api/health/info` | Informations du service |
| `GET` | `/swagger` | Documentation Swagger |

## 🔐 Authentification

### Endpoints publics (sans authentification)
- `/api/auth/login`
- `/api/auth/register-email`
- `/api/auth/register-verify-email`
- `/api/auth/register-complete`
- `/api/email/*` (tous les endpoints email)

### Endpoints protégés (authentification requise)
- `/api/auth/*` (sauf login et endpoints d'inscription)

### Utilisation du token JWT

```bash
# Exemple de requête authentifiée
curl -X GET "http://localhost:5000/api/auth/users/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📚 Documentation Swagger

### Accès à la documentation
- **Développement**: `http://localhost:5000/swagger`
- **Production**: `http://localhost:5000/api-docs`

### Fonctionnalités Swagger
- Documentation agrégée de tous les microservices
- Authentification JWT intégrée
- Tests d'API directement depuis l'interface
- Exemples de requêtes et réponses

## 📊 Logging

### Configuration Serilog
- **Console**: Logs en temps réel
- **Fichier**: Logs rotatifs dans `logs/api-gateway-YYYY-MM-DD.log`
- **Niveaux**: Information, Warning, Error

### Informations loggées
- Requêtes entrantes (méthode, path, headers, body)
- Réponses sortantes (status, headers, body, temps de réponse)
- Erreurs et exceptions
- Authentification et autorisation

## 🛡️ Sécurité

### CORS
- Configuration permissive pour le développement
- Configurable pour la production

### JWT Validation
- Validation complète des tokens (signature, expiration, issuer, audience)
- Gestion automatique des erreurs d'authentification

### Gestion d'Erreurs
- Réponses d'erreur standardisées
- Logs détaillés des erreurs
- Pas d'exposition d'informations sensibles

## 🚀 Déploiement

### Production
```bash
# Build pour la production
dotnet publish -c Release -o ./publish

# Variables d'environnement de production
export ASPNETCORE_ENVIRONMENT=Production
export JwtSettings__SecretKey=your-secure-production-key

# Démarrage
dotnet ./publish/ApiGateway.dll
```

### Docker Production
```bash
docker build -t pioloop-api-gateway:latest .
docker run -d \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e JwtSettings__SecretKey=your-secure-key \
  pioloop-api-gateway:latest
```

## 🧪 Tests

### Test de santé
```bash
curl http://localhost:5000/health
```

### Test d'authentification
```bash
# Login
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# Inscription (3 étapes)
curl -X POST "http://localhost:5000/api/auth/register-email" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'

# Utilisation du token
curl -X GET "http://localhost:5000/api/auth/users/1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test d'envoi d'email
```bash
curl -X POST "http://localhost:5000/api/email/send-email-verification" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","code":"123456"}'
```

## 🔧 Dépannage

### Problèmes courants

1. **Erreur de connexion aux microservices**
   - Vérifiez que les microservices sont démarrés
   - Vérifiez les ports dans `ocelot.json`

2. **Erreur JWT**
   - Vérifiez la configuration JWT dans `appsettings.json`
   - Assurez-vous que la clé secrète est identique dans tous les services

3. **Erreur Swagger**
   - Vérifiez que les microservices exposent leur Swagger
   - Vérifiez les URLs dans `ocelot.json`

### Logs
```bash
# Voir les logs en temps réel
tail -f logs/api-gateway-$(date +%Y-%m-%d).log
```

## 🤝 Contribution

1. Fork le repository
2. Créez une branche feature (`git checkout -b feature/amazing-feature`)
3. Committez vos changements (`git commit -m 'Add amazing feature'`)
4. Push vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrez une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](../../LICENSE) pour plus de détails.

---

**Note** : Ce API Gateway fait partie de l'architecture Pioloop. Pour plus d'informations sur l'architecture globale, consultez le [README principal](../../README.md).
