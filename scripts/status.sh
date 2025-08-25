#!/usr/bin/env bash
set -euo pipefail

# Modular status for Pioloop microservices using individual Docker Compose files.

# This script lives in pioloop-microservices/scripts
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"  # -> pioloop-microservices

# Define services
SERVICES=(
  "Auth.Microservice"
  "Email.Microservice"
  "Property.Microservice"
  "ApiGateway"
)

# Service ports mapping
declare -A SERVICE_PORTS=(
  ["Auth.Microservice"]="5001"
  ["Email.Microservice"]="5002"
  ["Property.Microservice"]="5003"
  ["ApiGateway"]="5000"
)

usage() {
  cat <<EOF
Usage: $0 [--service SERVICE_NAME]

Options:
  --service NAME   Show status for only the specified service

Available services:
  - Auth.Microservice
  - Email.Microservice
  - Property.Microservice
  - ApiGateway
EOF
}

SERVICE_FILTER=""

while [[ $# -gt 0 ]]; do
  case $1 in
    --service)
      SERVICE_FILTER="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1"
      usage
      exit 1
      ;;
  esac
done

check_service_status() {
  local service_dir="$1"
  local service_name=$(basename "$service_dir")
  local port="${SERVICE_PORTS[$service_name]}"
  
  echo "🔍 $service_name :"
  echo "----------------------"
  
  cd "$service_dir"
  
  if docker-compose ps | grep -q "Up"; then
    echo "[SUCCESS] ✅ $service_name is running"
    
    # Test connectivity
    if [[ -n "$port" ]]; then
      if curl -s -f "http://localhost:$port/health" >/dev/null 2>&1; then
        echo "[SUCCESS] ✅ API accessible on http://localhost:$port"
      else
        echo "[WARNING] ⚠️  API not responding on http://localhost:$port"
      fi
    fi
  else
    echo "[WARNING] ⚠️  $service_name is not running"
  fi
  
  echo ""
}

# Filter services if specified
if [[ -n "$SERVICE_FILTER" ]]; then
  if [[ ! " ${SERVICES[*]} " =~ " $SERVICE_FILTER " ]]; then
    echo "❌ Unknown service: $SERVICE_FILTER"
    echo "Available services: ${SERVICES[*]}"
    exit 1
  fi
  SERVICES=("$SERVICE_FILTER")
fi

echo "📊 Statut des microservices Pioloop (Architecture Modulaire)..."
echo "================================================================"
echo ""

# Check each service
for service in "${SERVICES[@]}"; do
  service_dir="$ROOT_DIR/$service"
  if [[ -d "$service_dir" ]]; then
    check_service_status "$service_dir"
  else
    echo "❌ Service directory not found: $service_dir"
  fi
done

echo "📋 URLs des services :"
echo "  • ApiGateway:     http://localhost:5000"
echo "  • Auth API:       http://localhost:5001"
echo "  • Email API:      http://localhost:5002"
echo "  • Property API:   http://localhost:5003"
echo ""
echo "🗄️  Database URLs :"
echo "  • Auth DB:        localhost:5433"
echo "  • Property DB:    localhost:5435"
echo ""
echo "🔧 Commandes utiles :"
echo "  • Démarrer tous:        ./scripts/start-all.sh"
echo "  • Arrêter tous:         ./scripts/stop-all.sh"
echo "  • Démarrer spécifique:  ./scripts/start-all.sh --service SERVICE_NAME"
echo "  • Logs:                 cd SERVICE_DIR && docker-compose logs -f"
