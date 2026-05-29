#!/usr/bin/env bash

set -e

FULL=false

if [[ "$1" == "--full" ]]; then
  FULL=true
fi

REMOTE_HOST="192.168.1.58"
REMOTE_USER="root"
REMOTE_PATH="/srv/deskmatch"

LOCAL_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "=== DeskMatch Deploy ==="
echo "Target: ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_PATH}"

echo
echo "[1/3] Ensuring remote base directory exists..."
ssh "${REMOTE_USER}@${REMOTE_HOST}" "mkdir -p ${REMOTE_PATH}"

echo
echo "[2/3] Syncing files (rsync)..."

rsync -avz \
  --delete \
  --delete-excluded \
  --exclude=".git" \
  --exclude="node_modules" \
  --exclude=".nuget" \
  --exclude="bin" \
  --exclude="obj" \
  --exclude="*.user" \
  --exclude="*.suo" \
  --exclude=".vs" \
  --exclude=".idea" \
  --exclude=".vscode" \
  --exclude="dist" \
  --exclude="logs" \
  "${LOCAL_PATH}/" \
  "${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_PATH}/"

echo
if [ "$FULL" = true ]; then
  echo "[3/3] Full rebuild + deploy..."

  ssh "${REMOTE_USER}@${REMOTE_HOST}" "
    cd ${REMOTE_PATH}/infrastructure/docker &&
    docker compose build --no-cache &&
    docker compose up -d
  "
else
  echo "[3/3] Fast deploy..."

  ssh "${REMOTE_USER}@${REMOTE_HOST}" "
    cd ${REMOTE_PATH}/infrastructure/docker &&
    docker compose up -d --build
  "
fi

echo
echo "=== Done ==="

ssh "${REMOTE_USER}@${REMOTE_HOST}" \
  "docker ps --filter name=deskmatch --format 'table {{.Names}}\t{{.Status}}'"

