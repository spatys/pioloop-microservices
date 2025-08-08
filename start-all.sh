#!/bin/bash

# Pioloop Microservices - Script de démarrage
# Ce script démarre tous les microservices dans l'ordre correct

set -e  # Arrêter en cas d'erreur

echo "🚀 Démarrage des microservices Pioloop..."
echo "========================================"

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonction pour afficher les messages colorés
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier que Docker est en cours d'exécution
if ! docker info > /dev/null 2>&1; then
    print_error "Docker n'est pas en cours d'exécution. Veuillez démarrer Docker."
    exit 1
fi

print_success "Docker est en cours d'exécution"

# Vérifier que les répertoires existent
if [ ! -d "Email.Microservice" ]; then
    print_error "Le répertoire Email.Microservice n'existe pas"
    exit 1
fi

if [ ! -d "Auth.Microservice" ]; then
    print_error "Le répertoire Auth.Microservice n'existe pas"
    exit 1
fi

print_success "Tous les répertoires microservices sont présents"

# Arrêter les services existants s'ils sont en cours d'exécution
print_status "Arrêt des services existants..."
cd Email.Microservice && docker-compose down > /dev/null 2>&1 || true
cd ../Auth.Microservice && docker-compose down > /dev/null 2>&1 || true
cd ..

# 1. Démarrer Email Microservice (en premier car Auth en dépend)
print_status "Démarrage du Email Microservice..."
cd Email.Microservice
docker-compose up -d

# Attendre que Email soit prêt
print_status "Attente que Email Microservice soit prêt..."
sleep 10

# Vérifier que Email est en cours d'exécution
if docker-compose ps | grep -q "Up"; then
    print_success "Email Microservice démarré avec succès"
else
    print_error "Échec du démarrage du Email Microservice"
    docker-compose logs
    exit 1
fi

cd ..

# 2. Démarrer Auth Microservice
print_status "Démarrage du Auth Microservice..."
cd Auth.Microservice
docker-compose up -d

# Attendre que Auth soit prêt
print_status "Attente que Auth Microservice soit prêt..."
sleep 15

# Vérifier que Auth est en cours d'exécution
if docker-compose ps | grep -q "Up"; then
    print_success "Auth Microservice démarré avec succès"
else
    print_error "Échec du démarrage du Auth Microservice"
    docker-compose logs
    exit 1
fi

cd ..

# Afficher le statut final
echo ""
echo "🎉 Tous les microservices sont démarrés !"
echo "========================================"
echo ""
echo "📋 URLs des services :"
echo "  • Auth API:     http://localhost:5001"
echo "  • Auth Swagger: http://localhost:5001/swagger"
echo "  • Email API:    http://localhost:5068"
echo "  • Email Swagger: http://localhost:5068/swagger"
echo "  • PostgreSQL:   localhost:5433"
echo ""
echo "🔧 Commandes utiles :"
echo "  • Voir les logs Auth:    cd Auth.Microservice && docker-compose logs -f"
echo "  • Voir les logs Email:   cd Email.Microservice && docker-compose logs -f"
echo "  • Arrêter tous:          ./stop-all.sh"
echo "  • Statut des services:   ./status.sh"
echo ""
print_success "Les microservices sont prêts pour les tests !"
