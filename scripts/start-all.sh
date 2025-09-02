#!/usr/bin/env bash
set -euo pipefail

# Script simple pour démarrer tous les microservices Pioloop
# Détecte automatiquement les services modifiés et les reconstruit

echo "🚀 Démarrage des microservices Pioloop..."

# Vérifier que Docker est en cours d'exécution
if ! docker info &> /dev/null; then
    echo "❌ Docker n'est pas en cours d'exécution. Veuillez démarrer Docker Desktop."
    exit 1
fi

# Vérifier que docker compose est disponible
if ! docker compose version &> /dev/null; then
    echo "❌ Docker Compose n'est pas disponible."
    exit 1
fi

# Aller au répertoire racine des microservices
cd "$(dirname "$0")/.."

# Vérifier que le fichier docker-compose.yml existe
if [[ ! -f "docker-compose.yml" ]]; then
    echo "❌ Fichier docker-compose.yml introuvable dans $(pwd)"
    exit 1
fi

echo "📍 Répertoire de travail: $(pwd)"

# Démarrer tous les services
echo "🔧 Démarrage de tous les services..."
docker compose up -d --build

echo ""
echo "✅ Tous les services ont été démarrés !"
echo ""
echo "🌐 URLs des services:"
echo "  • ApiGateway:     http://localhost:5000"
echo "  • Auth API:       http://localhost:5001"
echo "  • Email API:      http://localhost:5002"
echo "  • Property API:   http://localhost:5003"
echo ""
echo "🗄️  Bases de données:"
echo "  • Auth DB:        localhost:5433"
echo "  • Property DB:    localhost:5435"
echo ""
echo "📊 Statut des services:"
docker compose ps
