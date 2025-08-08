#!/bin/bash

# Pioloop Microservices - Script d'arrêt
# Ce script arrête tous les microservices

set -e  # Arrêter en cas d'erreur

echo "🛑 Arrêt des microservices Pioloop..."
echo "====================================="

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

# Vérifier que les répertoires existent
if [ ! -d "Email.Microservice" ]; then
    print_error "Le répertoire Email.Microservice n'existe pas"
    exit 1
fi

if [ ! -d "Auth.Microservice" ]; then
    print_error "Le répertoire Auth.Microservice n'existe pas"
    exit 1
fi

# Arrêter Auth Microservice
print_status "Arrêt du Auth Microservice..."
cd Auth.Microservice
docker-compose down
print_success "Auth Microservice arrêté"
cd ..

# Arrêter Email Microservice
print_status "Arrêt du Email Microservice..."
cd Email.Microservice
docker-compose down
print_success "Email Microservice arrêté"
cd ..

echo ""
print_success "Tous les microservices ont été arrêtés !"
echo ""
echo "💡 Pour supprimer aussi les volumes (données) :"
echo "  • Auth:    cd Auth.Microservice && docker-compose down -v"
echo "  • Email:   cd Email.Microservice && docker-compose down -v"
