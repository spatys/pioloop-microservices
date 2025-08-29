#!/usr/bin/env bash
set -euo pipefail

# Script pour arrêter et supprimer tous les containers, images et volumes Docker
# ATTENTION: Ce script supprime TOUT ce qui est lié à Docker

# Ce script vit dans pioloop-microservices/scripts
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"  # -> pioloop-microservices

# Fonction pour vérifier si Docker fonctionne
check_docker() {
  if ! docker info &> /dev/null; then
    echo "❌ Docker n'est pas en cours d'exécution"
    echo "Veuillez démarrer Docker et réessayer"
    exit 1
  fi
}

# Fonction pour nettoyer tout le système Docker
clean_everything() {
  echo "==> Nettoyage complet du système Docker..."
  
  # Arrêter tous les containers en cours d'exécution
  echo "  - Arrêt de tous les containers..."
  docker stop $(docker ps -q) 2>/dev/null || true
  
  # Supprimer tous les containers
  echo "  - Suppression de tous les containers..."
  docker rm -f $(docker ps -aq) 2>/dev/null || true
  
  # Supprimer toutes les images
  echo "  - Suppression de toutes les images..."
  docker rmi -f $(docker images -q) 2>/dev/null || true
  
  # Supprimer tous les volumes
  echo "  - Suppression de tous les volumes..."
  docker volume rm $(docker volume ls -q) 2>/dev/null || true
  
  # Supprimer tous les réseaux non utilisés
  echo "  - Suppression des réseaux non utilisés..."
  docker network prune -f 2>/dev/null || true
  
  # Nettoyer le système Docker (cache, etc.)
  echo "  - Nettoyage du système Docker..."
  docker system prune -af --volumes 2>/dev/null || true
  
  echo ""
  echo "✅ Nettoyage complet terminé !"
  echo "Tous les containers, images, volumes et réseaux non utilisés ont été supprimés."
}

# Vérifier que Docker fonctionne
check_docker

# Demander confirmation
echo "⚠️  ATTENTION: Ce script va supprimer TOUT ce qui est lié à Docker !"
echo "Cela inclut :"
echo "  - Tous les containers (arrêtés et en cours d'exécution)"
echo "  - Toutes les images"
echo "  - Tous les volumes"
echo "  - Tous les réseaux non utilisés"
echo "  - Tout le cache Docker"
echo ""
read -p "Êtes-vous sûr de vouloir continuer ? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
  echo "❌ Opération annulée"
  exit 0
fi

# Exécuter le nettoyage complet
clean_everything

echo ""
echo "🎉 Nettoyage terminé avec succès !"
