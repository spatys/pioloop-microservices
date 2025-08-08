# Pioloop Microservices

Ce répertoire contient les microservices de l'architecture Pioloop.

## Services disponibles

- **Auth Microservice** : Gestion de l'authentification et des utilisateurs
- **Email Microservice** : Service d'envoi d'emails

## Architecture

Chaque microservice est **complètement indépendant** avec son propre docker-compose :

```
Auth.Microservice/
├── docker-compose.yml (auth-api + auth-db)
└── Dockerfile

Email.Microservice/
├── docker-compose.yml (email-api)
└── Dockerfile
```

## 🚀 Démarrage rapide (Recommandé)

### Option 1: Script automatique (Recommandé)

```bash
# Démarrer tous les services en une commande
./start-all.sh

# Vérifier le statut
./status.sh

# Arrêter tous les services
./stop-all.sh
```

### Option 2: Démarrage manuel

#### 1. Démarrer Email Microservice (en premier)

```bash
cd Email.Microservice
docker-compose up -d
```

#### 2. Démarrer Auth Microservice

```bash
cd Auth.Microservice
docker-compose up -d
```

#### Vérifier le statut

```bash
# Vérifier Email
cd Email.Microservice && docker-compose ps

# Vérifier Auth
cd Auth.Microservice && docker-compose ps
```

## 📋 Scripts disponibles

| Script | Description |
|--------|-------------|
| `./start-all.sh` | Démarre tous les microservices dans l'ordre correct |
| `./stop-all.sh` | Arrête tous les microservices |
| `./status.sh` | Affiche le statut et teste la connectivité |

## URLs des services

- **Auth API** : http://localhost:5001
  - Swagger : http://localhost:5001/swagger
- **Email API** : http://localhost:5068
  - Swagger : http://localhost:5068/swagger
- **PostgreSQL** : localhost:5433

## Communication entre services

- **Auth → Email** : `http://localhost:5068` (communication externe)
- **Front → Auth** : `http://localhost:5001`
- **Front → Email** : `http://localhost:5068`

## Test d'inscription complet

1. **Inscription** :
   ```bash
   curl -X POST http://localhost:5001/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "firstName": "John",
       "lastName": "Doe", 
       "password": "password123",
       "confirmPassword": "password123",
       "acceptConsent": true
     }'
   ```

2. **Vérification email** (code reçu par email) :
   ```bash
   curl -X POST http://localhost:5001/api/auth/verify-email \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "code": "123456"
     }'
   ```

3. **Connexion** :
   ```bash
   curl -X POST http://localhost:5001/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "password": "password123"
     }'
   ```

## Configuration

### Variables d'environnement

- **Auth** : JWT, Database, EmailApi URL (localhost:5068)
- **Email** : SMTP settings (Gmail configuré)

### Réseaux Docker

- **Auth** : Réseau `auth-network` (auth-api + auth-db)
- **Email** : Réseau `email-network` (email-api)

## 🔧 Commandes utiles

### Gestion des services
```bash
# Démarrer tous
./start-all.sh

# Arrêter tous
./stop-all.sh

# Vérifier statut
./status.sh
```

### Logs et debugging
```bash
# Logs Auth
cd Auth.Microservice && docker-compose logs -f

# Logs Email
cd Email.Microservice && docker-compose logs -f

# Rebuild Auth
cd Auth.Microservice && docker-compose up --build -d

# Rebuild Email
cd Email.Microservice && docker-compose up --build -d
```

### Nettoyage
```bash
# Arrêter et supprimer les volumes (données)
cd Auth.Microservice && docker-compose down -v
cd Email.Microservice && docker-compose down -v

# Nettoyer les images Docker
docker system prune -f
```

## Avantages de cette approche

✅ **Indépendance totale** - Chaque service peut être déployé séparément
✅ **Équipes séparées** - Chaque équipe gère son propre compose
✅ **Isolation** - Problèmes dans un service n'affectent pas les autres
✅ **Scalabilité** - Peut scaler individuellement chaque service
✅ **Déploiements indépendants** - Peut déployer un service sans affecter l'autre
✅ **Scripts automatisés** - Démarrage/arrêt facile avec les scripts shell
