#!/bin/bash

# Pioloop Microservices - Script de statut
# Ce script affiche le statut de tous les microservices

echo "📊 Statut des microservices Pioloop..."
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

echo ""
echo "🔍 Email Microservice :"
echo "----------------------"
cd Email.Microservice
if docker-compose ps | grep -q "Up"; then
    print_success "✅ Email Microservice est en cours d'exécution"
    docker-compose ps
else
    print_warning "⚠️  Email Microservice n'est pas en cours d'exécution"
fi
cd ..

echo ""
echo "🔍 Auth Microservice :"
echo "---------------------"
cd Auth.Microservice
if docker-compose ps | grep -q "Up"; then
    print_success "✅ Auth Microservice est en cours d'exécution"
    docker-compose ps
else
    print_warning "⚠️  Auth Microservice n'est pas en cours d'exécution"
fi
cd ..

echo ""
echo "🌐 Test de connectivité :"
echo "------------------------"

# Test Email API
if curl -s http://localhost:5068/health > /dev/null 2>&1; then
    print_success "✅ Email API accessible sur http://localhost:5068"
else
    print_warning "⚠️  Email API non accessible sur http://localhost:5068"
fi

# Test Auth API
if curl -s http://localhost:5001/health > /dev/null 2>&1; then
    print_success "✅ Auth API accessible sur http://localhost:5001"
else
    print_warning "⚠️  Auth API non accessible sur http://localhost:5001"
fi

echo ""
echo "📋 URLs des services :"
echo "  • Auth API:     http://localhost:5001"
echo "  • Auth Swagger: http://localhost:5001/swagger"
echo "  • Email API:    http://localhost:5068"
echo "  • Email Swagger: http://localhost:5068/swagger"
echo "  • PostgreSQL:   localhost:5433"
echo ""
echo "🔧 Commandes utiles :"
echo "  • Démarrer tous:        ./start-all.sh"
echo "  • Arrêter tous:         ./stop-all.sh"
echo "  • Logs Auth:            cd Auth.Microservice && docker-compose logs -f"
echo "  • Logs Email:           cd Email.Microservice && docker-compose logs -f"
