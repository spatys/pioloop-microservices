#!/usr/bin/env bash
set -euo pipefail

# Modular start for Pioloop microservices using individual Docker Compose files.
# Each microservice manages its own containers independently.
# Includes incremental build detection for changed services.

# This script lives in pioloop-microservices/scripts
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"  # -> pioloop-microservices
STATE_DIR="$ROOT_DIR/.build-hash"
mkdir -p "$STATE_DIR"

# Create shared network if it doesn't exist
echo "==> Ensuring shared network exists..."
docker network create pioloop-network 2>/dev/null || echo "Pioloop network already exists"

# Define the order of startup (dependencies first)
SERVICES=(
  "Auth.Microservice"
  "Email.Microservice"
  "Property.Microservice"
  "ApiGateway"
)

usage() {
  cat <<EOF
Usage: $0 [--force] [--service SERVICE_NAME]

Options:
  --force          Rebuild and restart all services regardless of changes.
  --service NAME   Start only the specified service (e.g., Auth.Microservice)

Available services:
  - Auth.Microservice
  - Email.Microservice
  - Property.Microservice
  - ApiGateway

Notes:
  - Services are started in dependency order
  - Each service manages its own containers
  - Only changed services are rebuilt (unless --force is used)
EOF
}

FORCE=0
SERVICE_FILTER=""

while [[ $# -gt 0 ]]; do
  case $1 in
    --force)
      FORCE=1
      shift
      ;;
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

# Function to compute hash of a directory (excluding build artifacts)
hash_dir() {
  local dir="$1"
  # shellcheck disable=SC2016
  find "$dir" -type f \
    ! -path '*/bin/*' \
    ! -path '*/obj/*' \
    ! -path '*/logs/*' \
    ! -path '*/.git/*' \
    ! -path '*/node_modules/*' \
    ! -path '*/.build-hash/*' \
    -print0 | sort -z | xargs -0 sha256sum | sha256sum | awk '{print $1}'
}

# Function to detect changed services
detect_changes() {
  local changed_services=()
  
  echo "==> Computing service hashes..."
  for service in "${SERVICES[@]}"; do
    service_dir="$ROOT_DIR/$service"
    if [[ -d "$service_dir" ]]; then
      current_hash="$(hash_dir "$service_dir" 2>/dev/null || echo 'missing')"
      state_file="$STATE_DIR/$service.hash"
      previous_hash="$(cat "$state_file" 2>/dev/null || echo 'none')"

      if [[ "$FORCE" -eq 1 ]] || [[ "$current_hash" != "$previous_hash" ]]; then
        echo "  - $service: changed"
        changed_services+=("$service")
      else
        echo "  - $service: unchanged"
      fi
    fi
  done
  
  echo "${changed_services[@]}"
}

start_service() {
  local service_dir="$1"
  local service_name=$(basename "$service_dir")
  local should_rebuild="$2"
  
  echo "==> Starting $service_name..."
  cd "$service_dir"
  
  if [[ "$should_rebuild" == "true" ]]; then
    echo "  - Rebuilding and restarting (changes detected)"
    docker-compose down
    docker-compose up -d --build
  else
    echo "  - Starting if not running (no changes)"
    docker-compose up -d
  fi
  
  echo "  - ✅ $service_name started"
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

echo "==> Starting Pioloop microservices..."
echo "Services to start: ${SERVICES[*]}"
echo "Force rebuild: $([[ $FORCE -eq 1 ]] && echo "Yes" || echo "No")"
echo ""

# Detect changes
IFS=' ' read -r -a changed_services <<< "$(detect_changes)"

# Start services in order
for service in "${SERVICES[@]}"; do
  service_dir="$ROOT_DIR/$service"
  if [[ -d "$service_dir" ]]; then
    # Check if this service needs rebuilding
    should_rebuild="false"
    if [[ "$FORCE" -eq 1 ]] || [[ " ${changed_services[*]} " =~ " $service " ]]; then
      should_rebuild="true"
    fi
    
    start_service "$service_dir" "$should_rebuild"
    
    # Update hash if service was rebuilt
    if [[ "$should_rebuild" == "true" ]]; then
      hash_dir "$service_dir" > "$STATE_DIR/$service.hash" 2>/dev/null || true
    fi
  else
    echo "❌ Service directory not found: $service_dir"
  fi
done

echo ""
echo "==> All services started!"
echo ""
echo "📊 Service URLs:"
echo "  • ApiGateway:     http://localhost:5000"
echo "  • Auth API:       http://localhost:5001"
echo "  • Email API:      http://localhost:5002"
echo "  • Property API:   http://localhost:5003"
echo ""
echo "🗄️  Database URLs:"
echo "  • Auth DB:        localhost:5433"
echo "  • Property DB:    localhost:5435"
echo ""
echo "🔧 Useful commands:"
echo "  • Stop all:       ./scripts/stop-all.sh"
echo "  • Status:         ./scripts/status.sh"
echo "  • Logs:           docker-compose logs -f (in service directory)"
