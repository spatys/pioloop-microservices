#!/usr/bin/env bash
set -euo pipefail

# Single Docker Compose management for Pioloop microservices
# This script manages all microservices using the unified docker-compose.yml
# Includes intelligent change detection to only rebuild modified services

# This script lives in pioloop-microservices/scripts
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"  # -> pioloop-microservices
STATE_DIR="$ROOT_DIR/.build-hash"
mkdir -p "$STATE_DIR"

usage() {
  cat <<EOF
Usage: $0 [COMMAND] [OPTIONS]

Commands:
  up              Start all services (only rebuilds modified services)
  down            Stop all services
  restart         Restart all services
  logs            Show logs for all services
  logs SERVICE    Show logs for specific service
  status          Show status of all services
  build           Build all services
  build SERVICE   Build specific service
  clean           Remove all containers, networks, and volumes

Options:
  --force         Force rebuild all services (ignores change detection)
  --detach        Run in background (for up command)
  --follow        Follow logs (for logs command)
  --service       Specify a single service to operate on

Available services:
  - api-gateway
  - auth-api
  - property-api
  - email-api
  - auth-db
  - property-db

Examples:
  $0 up --detach                    # Start all services in background (only rebuilds modified)
  $0 up --force                     # Force rebuild and start all services
  $0 up --service auth-api          # Start only auth-api service
  $0 build --force                  # Force rebuild all services
  $0 logs api-gateway --follow      # Follow api-gateway logs
  $0 status                         # Show all services status
EOF
}

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
  local services=("api-gateway" "auth-api" "property-api" "email-api")
  
  echo "==> Computing service hashes..." >&2
  for service in "${services[@]}"; do
    case $service in
      "api-gateway")
        service_dir="$ROOT_DIR/ApiGateway"
        ;;
      "auth-api")
        service_dir="$ROOT_DIR/Auth.Microservice"
        ;;
      "property-api")
        service_dir="$ROOT_DIR/Property.Microservice"
        ;;
      "email-api")
        service_dir="$ROOT_DIR/Email.Microservice"
        ;;
    esac
    
    if [[ -d "$service_dir" ]]; then
      current_hash="$(hash_dir "$service_dir" 2>/dev/null || echo 'missing')"
      state_file="$STATE_DIR/$service.hash"
      previous_hash="$(cat "$state_file" 2>/dev/null || echo 'none')"

      if [[ "$current_hash" != "$previous_hash" ]]; then
        echo "  - $service: changed" >&2
        changed_services+=("$service")
      else
        echo "  - $service: unchanged" >&2
      fi
    fi
  done
  
  # Return the array as space-separated string
  printf '%s ' "${changed_services[@]}"
}

# Function to update service hash
update_service_hash() {
  local service="$1"
  local service_dir=""
  
  case $service in
    "api-gateway")
      service_dir="$ROOT_DIR/ApiGateway"
      ;;
    "auth-api")
      service_dir="$ROOT_DIR/Auth.Microservice"
      ;;
    "property-api")
      service_dir="$ROOT_DIR/Property.Microservice"
      ;;
    "email-api")
      service_dir="$ROOT_DIR/Email.Microservice"
      ;;
  esac
  
  if [[ -d "$service_dir" ]]; then
    hash_dir "$service_dir" > "$STATE_DIR/$service.hash" 2>/dev/null || true
  fi
}

# Function to check if docker-compose is available
check_docker_compose() {
  if ! command -v docker-compose &> /dev/null; then
    echo "❌ docker-compose is not installed or not in PATH"
    echo "Please install docker-compose and try again"
    exit 1
  fi
}

# Function to check if Docker is running
check_docker() {
  if ! docker info &> /dev/null; then
    echo "❌ Docker is not running"
    echo "Please start Docker and try again"
    exit 1
  fi
}

# Function to show service status
show_status() {
  echo "==> Service Status:"
  docker-compose -f "$ROOT_DIR/docker-compose.yml" ps
  echo ""
  echo "==> Service URLs:"
  echo "  • ApiGateway:     http://localhost:5000"
  echo "  • Auth API:       http://localhost:5001"
  echo "  • Email API:      http://localhost:5002"
  echo "  • Property API:   http://localhost:5003"
  echo ""
  echo "🗄️  Database URLs:"
echo "  • Auth DB:        localhost:5433"
echo "  • Property DB:    localhost:5435"
}

# Function to show logs
show_logs() {
  local service="$1"
  local follow="$2"
  
  if [[ -n "$service" ]]; then
    echo "==> Showing logs for $service..."
    docker-compose -f "$ROOT_DIR/docker-compose.yml" logs $follow "$service"
  else
    echo "==> Showing logs for all services..."
    docker-compose -f "$ROOT_DIR/docker-compose.yml" logs $follow
  fi
}

# Function to build services
build_services() {
  local service="$1"
  local force="$2"
  
  if [[ -n "$service" ]]; then
    echo "==> Building $service..."
    docker-compose -f "$ROOT_DIR/docker-compose.yml" build $force "$service"
  else
    echo "==> Building all services..."
    docker-compose -f "$ROOT_DIR/docker-compose.yml" build $force
  fi
}

# Function to clean everything
clean_all() {
  echo "⚠️  This will remove all containers, networks, and volumes!"
  read -p "Are you sure? (y/N): " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "==> Cleaning all Docker resources..."
    docker-compose -f "$ROOT_DIR/docker-compose.yml" down -v --remove-orphans
    docker system prune -f
    echo "✅ Cleanup completed"
  else
    echo "❌ Cleanup cancelled"
  fi
}

# Main script logic
main() {
  # Check prerequisites
  check_docker
  check_docker_compose
  
  # Parse command
  case "${1:-}" in
    up)
      local detach=""
      local force=""
      local service_filter=""
      
      # Parse options
      shift
      while [[ $# -gt 0 ]]; do
        case $1 in
          --detach)
            detach="-d"
            shift
            ;;
          --force)
            force="true"
            shift
            ;;
          --service)
            service_filter="$2"
            shift 2
            ;;
          *)
            echo "❌ Unknown option: $1"
            usage
            exit 1
            ;;
        esac
      done
      
      if [[ -n "$service_filter" ]]; then
        # Start specific service
        echo "==> Starting service: $service_filter..."
        docker-compose -f "$ROOT_DIR/docker-compose.yml" up $detach "$service_filter"
      else
        # Start all services with change detection
        if [[ "$force" == "true" ]]; then
          echo "==> Force rebuilding and starting all services..."
          docker-compose -f "$ROOT_DIR/docker-compose.yml" up $detach --build
        else
          echo "==> Starting services with change detection..."
          
          # Detect changed services
          changed_services_output=$(detect_changes)
          read -r -a changed_services <<< "$changed_services_output"
          
          if [[ ${#changed_services[@]} -eq 0 ]] || [[ -z "$changed_services_output" ]]; then
            echo "==> No changes detected, starting existing containers..."
            docker-compose -f "$ROOT_DIR/docker-compose.yml" up $detach
          else
            echo "==> Changes detected in: ${changed_services[*]}"
            echo "==> Rebuilding changed services and starting all..."
            
            # Build only changed services
            for service in "${changed_services[@]}"; do
              if [[ -n "$service" ]]; then
                echo "  - Rebuilding $service..."
                docker-compose -f "$ROOT_DIR/docker-compose.yml" build "$service"
                update_service_hash "$service"
              fi
            done
            
            # Start all services
            docker-compose -f "$ROOT_DIR/docker-compose.yml" up $detach
          fi
        fi
      fi
      
      if [[ -z "$detach" ]]; then
        echo ""
        show_status
      fi
      ;;
    down)
      echo "==> Stopping all services..."
      docker-compose -f "$ROOT_DIR/docker-compose.yml" down
      ;;
    restart)
      local service_filter=""
      if [[ "${2:-}" == "--service" ]]; then
        service_filter="${3:-}"
      fi
      
      if [[ -n "$service_filter" ]]; then
        echo "==> Restarting service: $service_filter..."
        docker-compose -f "$ROOT_DIR/docker-compose.yml" restart "$service_filter"
      else
        echo "==> Restarting all services..."
        docker-compose -f "$ROOT_DIR/docker-compose.yml" restart
      fi
      ;;
    logs)
      local service="${2:-}"
      local follow=""
      if [[ "${3:-}" == "--follow" ]]; then
        follow="-f"
      fi
      show_logs "$service" "$follow"
      ;;
    status)
      show_status
      ;;
    build)
      local service="${2:-}"
      local force=""
      if [[ "${3:-}" == "--force" ]]; then
        force="--no-cache"
      fi
      build_services "$service" "$force"
      
      # Update hash if building specific service
      if [[ -n "$service" ]]; then
        update_service_hash "$service"
      fi
      ;;
    clean)
      clean_all
      ;;
    -h|--help|help)
      usage
      ;;
    *)
      echo "❌ Unknown command: ${1:-}"
      echo ""
      usage
      exit 1
      ;;
  esac
}

# Run main function with all arguments
main "$@"
