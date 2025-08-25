#!/usr/bin/env bash
set -euo pipefail

# Modular stop for Pioloop microservices using individual Docker Compose files.

# This script lives in pioloop-microservices/scripts
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"  # -> pioloop-microservices

# Define services in reverse order (dependencies last)
SERVICES=(
  "ApiGateway"
  "Property.Microservice"
  "Email.Microservice"
  "Auth.Microservice"
)

usage() {
  cat <<EOF
Usage: $0 [--service SERVICE_NAME]

Options:
  --service NAME   Stop only the specified service (e.g., Auth.Microservice)

Available services:
  - ApiGateway
  - Auth.Microservice
  - Email.Microservice
  - Property.Microservice

Notes:
  - Services are stopped in reverse dependency order
  - Each service manages its own containers
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

stop_service() {
  local service_dir="$1"
  local service_name=$(basename "$service_dir")
  
  echo "==> Stopping $service_name..."
  cd "$service_dir"
  
  if docker-compose ps | grep -q "Up"; then
    docker-compose down
    echo "  - ✅ $service_name stopped"
  else
    echo "  - ⏭️  $service_name already stopped"
  fi
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

echo "==> Stopping Pioloop microservices..."
echo "Services to stop: ${SERVICES[*]}"
echo ""

# Stop services in reverse order
for service in "${SERVICES[@]}"; do
  service_dir="$ROOT_DIR/$service"
  if [[ -d "$service_dir" ]]; then
    stop_service "$service_dir"
  else
    echo "❌ Service directory not found: $service_dir"
  fi
done

echo ""
echo "==> All services stopped!"
echo ""
echo "🔧 To start services again:"
echo "  • Start all:       ./scripts/start-all-modular.sh"
echo "  • Start specific:  ./scripts/start-all-modular.sh --service SERVICE_NAME"
