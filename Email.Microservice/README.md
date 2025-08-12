# Email Microservice

Service dédié à l'envoi d'emails dans l'écosystème Pioloop.

## 🚀 Fonctionnalités

- Envoi d'emails de vérification
- Emails de bienvenue pour nouveaux comptes
- Emails de réinitialisation de mot de passe
- Confirmations de réservation
- Confirmations de paiement
- Envoi de factures
- Envoi de contrats

## 🏗️ Architecture

```
Email.Microservice/
├── src/
│   ├── Email.API/           # Couche API (Controllers, Program.cs)
│   ├── Email.Application/   # Couche Application (Handlers, DTOs)
│   ├── Email.Domain/        # Couche Domain (Interfaces)
│   └── Email.Infrastructure/# Couche Infrastructure (Services)
```

## 📋 Prérequis

- .NET 8.0 SDK
- Docker (optionnel)
- Compte SMTP (Gmail, SendGrid, etc.)

## 🛠️ Installation

### Développement local

```bash
cd Email.Microservice
dotnet restore
dotnet build
dotnet run --project src/Email.API
```

### Avec Docker

```bash
docker build -t email-microservice .
docker run -p 5002:80 email-microservice
```

### Avec Docker Compose

```bash
docker-compose up -d
```

## 🔧 Configuration

### Variables d'environnement

```bash
# SMTP Configuration
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=your-email@gmail.com
EmailSettings__SmtpPassword=your-app-password
EmailSettings__FromEmail=noreply@pioloop.com
EmailSettings__FromName=Pioloop

# CORS Origins
CORS__AllowedOrigins__0=https://api.pioloop.com
CORS__AllowedOrigins__1=https://pioloop.com
```

### appsettings.json

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@pioloop.com",
    "FromName": "Pioloop"
  }
}
```

## 📡 API Endpoints

### Base URL
- **Développement**: `http://localhost:5002`
- **Production**: `https://api.pioloop.com`

### Endpoints

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `POST` | `/api/email/send-email-verification` | Envoi d'email de vérification |
| `POST` | `/api/email/send-email-account-created` | Email de bienvenue |
| `POST` | `/api/email/send-email-password-reset` | Email de réinitialisation |
| `POST` | `/api/email/send-email-reservation-confirmation` | Confirmation de réservation |
| `POST` | `/api/email/send-email-payment-confirmation` | Confirmation de paiement |
| `POST` | `/api/email/send-email-invoice` | Envoi de facture |
| `POST` | `/api/email/send-email-contract` | Envoi de contrat |
| `GET` | `/api/email/health` | Vérification de santé |
| `GET` | `/api/email/info` | Informations du service |

### Documentation Swagger

- **Développement**: `http://localhost:5002/`
- **Production**: `https://api.pioloop.com/api-docs`

## 📝 Exemples d'utilisation

### Envoi d'email de vérification

```bash
curl -X POST "http://localhost:5002/api/email/send-email-verification" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "code": "123456"
  }'
```

### Envoi d'email de bienvenue

```bash
curl -X POST "http://localhost:5002/api/email/send-email-account-created" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

## 🔒 Sécurité

### CORS
Le service accepte les requêtes depuis :
- `https://api.pioloop.com` (API Gateway uniquement)

**Méthodes HTTP autorisées :**
- `GET` - Lecture des données
- `POST` - Création de données
- `PUT` - Mise à jour complète
- `DELETE` - Suppression de données
- `PATCH` - Mise à jour partielle

**Note de sécurité :** Seul l'API Gateway a accès direct aux microservices. Les clients frontend doivent passer par l'API Gateway.

### Validation
- Validation des adresses email
- Validation des paramètres requis
- Gestion d'erreurs structurée

## 📊 Monitoring

### Health Check
```bash
curl http://localhost:5002/api/email/health
```

### Informations du service
```bash
curl http://localhost:5002/api/email/info
```

## 🐛 Dépannage

### Erreurs courantes

1. **Erreur SMTP**
   - Vérifiez les paramètres SMTP
   - Assurez-vous que l'authentification à 2 facteurs est activée pour Gmail
   - Utilisez un mot de passe d'application pour Gmail

2. **Erreur CORS**
   - Vérifiez que l'origine est autorisée
   - Assurez-vous que le service est accessible

3. **Erreur de validation**
   - Vérifiez le format de l'email
   - Assurez-vous que tous les champs requis sont fournis

## 🤝 Contribution

1. Fork le projet
2. Créez une branche feature (`git checkout -b feature/AmazingFeature`)
3. Committez vos changements (`git commit -m 'Add some AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrez une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

## 📞 Support

- **Email**: support@pioloop.com
- **Site web**: https://pioloop.com
- **Documentation**: https://docs.pioloop.com
