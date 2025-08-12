#!/bin/bash

# Pioloop Microservices - Script d'arrêt Docker Compose
# Ce script arrête l'écosystème complet avec Docker Compose

set -e  # Arrêter en cas d'erreur

echo "🛑 Arrêt de l'écosystème Pioloop Docker Compose..."
echo "=================================================="

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

# Vérifier que Docker Compose est installé
print_status "Vérification de Docker Compose..."
if ! command -v docker-compose &> /dev/null; then
    print_error "Docker Compose n'est pas installé"
    exit 1
fi

# Vérifier que le répertoire ApiGateway existe
if [ ! -d "ApiGateway" ]; then
    print_error "Le répertoire ApiGateway n'existe pas"
    exit 1
fi

# Arrêter les services
print_status "Arrêt des services Docker Compose..."
cd ApiGateway

# Arrêter les services en cours d'exécution
if docker-compose ps | grep -q "Up"; then
    print_status "Arrêt des conteneurs en cours d'exécution..."
    docker-compose down
    print_success "Services arrêtés"
else
    print_warning "Aucun service en cours d'exécution"
fi

# Supprimer les volumes (optionnel)
read -p "Voulez-vous supprimer les volumes de données ? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Suppression des volumes..."
    docker-compose down -v
    print_success "Volumes supprimés"
fi

# Nettoyer les images non utilisées
read -p "Voulez-vous nettoyer les images Docker non utilisées ? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Nettoyage des images Docker..."
    docker system prune -f
    print_success "Images nettoyées"
fi

cd ..

echo ""
print_success "🎉 Écosystème Pioloop arrêté avec succès !"
echo ""
echo "💡 Pour redémarrer :"
echo "  • ./start-all.sh"
echo ""
echo "💡 Pour voir les conteneurs :"
echo "  • docker ps -a"
echo ""
