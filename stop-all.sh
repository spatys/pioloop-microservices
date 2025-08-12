#!/bin/bash

echo "🛑 Arrêt de l'écosystème Pioloop..."

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

# Arrêter l'API Gateway en premier
print_status "Arrêt de l'API Gateway..."
pkill -f "dotnet.*ApiGateway" || print_warning "API Gateway n'était pas en cours d'exécution"

# Arrêter les microservices
print_status "Arrêt des microservices..."

# Auth Microservice
print_status "Arrêt de Auth.Microservice..."
pkill -f "dotnet.*Auth.Microservice" || print_warning "Auth.Microservice n'était pas en cours d'exécution"

# Email Microservice
print_status "Arrêt de Email.Microservice..."
pkill -f "dotnet.*Email.Microservice" || print_warning "Email.Microservice n'était pas en cours d'exécution"

# Attendre que les processus se terminent
sleep 3

# Vérifier s'il reste des processus
remaining_processes=$(pgrep -f "dotnet.*(Auth|Email|ApiGateway)" || true)

if [ -n "$remaining_processes" ]; then
    print_warning "Forçage de l'arrêt des processus restants..."
    echo "$remaining_processes" | xargs kill -9
    sleep 2
fi

# Vérifier que tous les ports sont libres
ports=(5000 5001 5002)
all_ports_free=true

for port in "${ports[@]}"; do
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        print_error "Le port $port est encore utilisé"
        all_ports_free=false
    else
        print_success "Port $port libéré"
    fi
done

echo ""
if [ "$all_ports_free" = true ]; then
    print_success "🎉 Tous les services ont été arrêtés avec succès !"
else
    print_warning "⚠️  Certains ports pourraient encore être utilisés"
fi

echo ""
print_status "Nettoyage des fichiers PID..."
rm -f logs/*.pid

echo ""
print_success "Écosystème Pioloop arrêté"
