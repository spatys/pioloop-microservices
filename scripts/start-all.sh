#!/usr/bin/env bash
set -euo pipefail

# Incremental start for Pioloop microservices using Docker Compose.
# Restarts/builds ONLY services whose source code changed since the last successful run.

# This script lives in pioloop-microservices/scripts
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"  # -> pioloop-microservices
COMPOSE_DIR="$ROOT_DIR/ApiGateway"
STATE_DIR="$ROOT_DIR/.build-hash"
mkdir -p "$STATE_DIR"

# Map: compose service name -> source directory to watch for changes
declare -A SERVICE_DIRS=(
  [api-gateway]="$ROOT_DIR/ApiGateway"
  [auth-microservice]="$ROOT_DIR/Auth.Microservice"
  [email-microservice]="$ROOT_DIR/Email.Microservice"
)

usage() {
  cat <<EOF
Usage: $0 [--force]

Options:
  --force   Rebuild and restart all services regardless of detected changes.

Notes:
  - Hash excludes bin/ obj/ logs/ and .git/ folders.
  - Untracked local changes are considered.
EOF
}

FORCE=0
if [[ "${1:-}" == "--force" ]]; then
  FORCE=1
elif [[ -n "${1:-}" ]]; then
  usage
  exit 1
fi

hash_dir() {
  local dir="$1"
  # shellcheck disable=SC2016
  find "$dir" -type f \
    ! -path '*/bin/*' \
    ! -path '*/obj/*' \
    ! -path '*/logs/*' \
    ! -path '*/.git/*' \
    -print0 | sort -z | xargs -0 sha256sum | sha256sum | awk '{print $1}'
}

changed_services=()

echo "==> Computing service hashes..."
for svc in "${!SERVICE_DIRS[@]}"; do
  src_dir="${SERVICE_DIRS[$svc]}"
  current_hash="$(hash_dir "$src_dir" || echo 'missing')"
  state_file="$STATE_DIR/$svc.hash"
  previous_hash="$(cat "$state_file" 2>/dev/null || echo 'none')"

  if [[ "$FORCE" -eq 1 ]] || [[ "$current_hash" != "$previous_hash" ]]; then
    echo "  - $svc: changed"
    changed_services+=("$svc")
  else
    echo "  - $svc: unchanged"
  fi
done

cd "$COMPOSE_DIR"

if (( ${#changed_services[@]} > 0 )); then
  echo "==> Building changed services: ${changed_services[*]}"
  docker compose build "${changed_services[@]}"

  echo "==> Restarting changed services without dependencies"
  docker compose up -d --no-deps "${changed_services[@]}"

  echo "==> Updating state hashes"
  for svc in "${changed_services[@]}"; do
    hash_dir "${SERVICE_DIRS[$svc]}" > "$STATE_DIR/$svc.hash" || true
  done
else
  echo "==> No changes detected. Skipping builds and restarts."
fi

echo "==> Ensuring all services are up (without recreation)"
docker compose up -d --no-recreate

echo "==> Done."


