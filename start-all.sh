#!/bin/bash

echo "🚀 Démarrage de l'écosystème Pioloop..."

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

# Vérifier que .NET 8.0 est installé
print_status "Vérification de .NET 8.0..."
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK n'est pas installé"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
if [[ ! $DOTNET_VERSION =~ ^(8\.|9\.) ]]; then
    print_error ".NET 8.0+ est requis, version actuelle: $DOTNET_VERSION"
    exit 1
fi

print_success ".NET $DOTNET_VERSION détecté"

# Créer le dossier logs s'il n'existe pas
mkdir -p logs

# Fonction pour démarrer un service
start_service() {
    local service_name=$1
    local service_path=$2
    local port=$3
    
    print_status "Démarrage de $service_name sur le port $port..."
    
    # Vérifier si le port est déjà utilisé
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null ; then
        print_warning "Le port $port est déjà utilisé. Arrêt du processus existant..."
        lsof -ti:$port | xargs kill -9
        sleep 2
    fi
    
    # Démarrer le service en arrière-plan
    cd "$service_path"
    dotnet run --no-build > "../logs/$service_name.log" 2>&1 &
    local pid=$!
    echo $pid > "../logs/$service_name.pid"
    cd ..
    
    # Attendre que le service démarre
    sleep 5
    
    # Vérifier si le service répond
    if curl -s "http://localhost:$port/health" > /dev/null 2>&1; then
        print_success "$service_name démarré avec succès (PID: $pid)"
    else
        print_warning "$service_name pourrait ne pas être complètement démarré"
    fi
}

# Arrêter les services existants
print_status "Arrêt des services existants..."
pkill -f "dotnet.*Auth.Microservice" || true
pkill -f "dotnet.*Email.Microservice" || true
pkill -f "dotnet.*ApiGateway" || true

# Attendre que les processus se terminent
sleep 3

# Démarrer les microservices
print_status "Démarrage des microservices..."

# Auth Microservice
start_service "Auth.Microservice" "Auth.Microservice" 5001

# Email Microservice  
start_service "Email.Microservice" "Email.Microservice" 5002

# Attendre un peu avant de démarrer l'API Gateway
sleep 3

# API Gateway
start_service "ApiGateway" "ApiGateway" 5000

# Attendre que tous les services démarrent
sleep 5

# Vérifier l'état de tous les services
print_status "Vérification de l'état des services..."

services=(
    "Auth.Microservice:5001"
    "Email.Microservice:5002"
    "ApiGateway:5000"
)

all_healthy=true

for service in "${services[@]}"; do
    IFS=':' read -r name port <<< "$service"
    
    if curl -s "http://localhost:$port/health" > /dev/null 2>&1; then
        print_success "$name est en ligne (http://localhost:$port)"
    else
        print_error "$name n'est pas accessible sur le port $port"
        all_healthy=false
    fi
done

echo ""
if [ "$all_healthy" = true ]; then
    print_success "🎉 Tous les services sont démarrés et opérationnels !"
    echo ""
    echo "📋 URLs des services :"
    echo "  🔐 Auth Service:     http://localhost:5001"
    echo "  📧 Email Service:    http://localhost:5002"
    echo "  🌐 API Gateway:      http://localhost:5000"
    echo ""
    echo "📚 Documentation Swagger :"
    echo "  🔐 Auth Swagger:     http://localhost:5001/"
    echo "  📧 Email Swagger:    http://localhost:5002/"
    echo "  🌐 Gateway Swagger:  http://localhost:5000/swagger"
    echo ""
    echo "🔍 Logs disponibles dans le dossier 'logs/'"
    echo "🛑 Pour arrêter tous les services: ./stop-all.sh"
else
    print_error "❌ Certains services ne sont pas accessibles"
    echo "Vérifiez les logs dans le dossier 'logs/' pour plus de détails"
fi

echo ""
